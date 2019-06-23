using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace Opw.HttpExceptions.AspNetCore
{
    public class HttpExceptionsMiddlewareTests
    {
        private readonly HttpExceptionsMiddleware _middleware;
        private readonly Mock<RequestDelegate> _nextMock;
        private readonly Mock<IActionResultExecutor<ObjectResult>> _actionResultExecutorMock;

        public HttpExceptionsMiddlewareTests()
        {
            _nextMock = new Mock<RequestDelegate>();
            var optionsMock = TestHelper.CreateHttpExceptionsOptionsMock(true);
            _actionResultExecutorMock = new Mock<IActionResultExecutor<ObjectResult>>();
            var loggerMock = new Mock<ILogger<HttpExceptionsMiddleware>>();
            _middleware = new HttpExceptionsMiddleware(_nextMock.Object, optionsMock.Object, _actionResultExecutorMock.Object, loggerMock.Object);
        }

        [Fact]
        public async Task Invoke_Should_ReturnProblemDetailsResult_ForApplicationException_WhenExceptionThrown()
        {
            _nextMock.Setup(n => n.Invoke(It.IsAny<HttpContext>())).Throws(new ApplicationException());
            ProblemDetailsResult result = null;
            _actionResultExecutorMock.Setup(e => e.ExecuteAsync(It.IsAny<ActionContext>(), It.IsAny<ObjectResult>()))
                .Callback<ActionContext, ObjectResult>((actionContext, actionResult) => result = (ProblemDetailsResult)actionResult)
                .Returns(Task.CompletedTask);

            await _middleware.Invoke(new DefaultHttpContext());

            result.Should().NotBeNull();
            result.StatusCode.Should().Be((int)HttpStatusCode.InternalServerError);
            result.Value.ShouldNotBeNull(HttpStatusCode.InternalServerError);
        }

        [Fact]
        public async Task Invoke_Should_ReturnProblemDetailsResult_WhenUnauthorizedRequest()
        {
            _nextMock.Setup(n => n.Invoke(It.IsAny<HttpContext>()))
                .Returns((HttpContext context) => {
                    context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    return Task.CompletedTask;
                });

            ProblemDetailsResult result = null;
            _actionResultExecutorMock.Setup(e => e.ExecuteAsync(It.IsAny<ActionContext>(), It.IsAny<ObjectResult>()))
                .Callback<ActionContext, ObjectResult>((actionContext, actionResult) => result = (ProblemDetailsResult)actionResult)
                .Returns(Task.CompletedTask);

            await _middleware.Invoke(new DefaultHttpContext());

            result.Should().NotBeNull();
            result.StatusCode.Should().Be((int)HttpStatusCode.Unauthorized);
            result.Value.ShouldNotBeNull(HttpStatusCode.Unauthorized);
            result.Value.Title.Should().Be("Unauthorized");
        }
    }
}