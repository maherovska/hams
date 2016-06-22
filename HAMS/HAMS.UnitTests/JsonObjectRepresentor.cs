using Rhino.Mocks;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace HAMS.UnitTests
{
    public static class JsonObjectRepresentor
    {
        public static T GetJsonObjectRepresentation<T>(JsonResult jsonResult)
        {
            var controllerContextMock = Rhino.Mocks.MockRepository.GenerateStub<ControllerContext>();
            var httpContextMock = Rhino.Mocks.MockRepository.GenerateStub<HttpContextBase>();
            var httpResponseMock = Rhino.Mocks.MockRepository.GenerateStub<HttpResponseBase>();

            httpContextMock.Stub(x => x.Response).Return(httpResponseMock);
            controllerContextMock.HttpContext = httpContextMock;

            jsonResult.ExecuteResult(controllerContextMock);

            var args = httpResponseMock.GetArgumentsForCallsMadeOn(x => x.Write(null));

            JavaScriptSerializer serializer = new JavaScriptSerializer();
            return serializer.Deserialize<T>(args[0][0] as string);
        }
    }
}
