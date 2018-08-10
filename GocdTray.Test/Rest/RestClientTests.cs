﻿using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using GocdTray.App;
using GocdTray.App.Abstractions;
using GocdTray.Rest;
using Newtonsoft.Json;
using NUnit.Framework;

namespace GocdTray.Test.Rest
{
    [TestFixture]
    public class RestClientTests
    {
        public class TestObject
        {
            public string Name { get; set; }
            public int Value { get; set; }
        }

        [Test]
        public void Get_IfTheRequestIsInvalid_TheExceptionIsCaught_AndTheMessagesReturnedInTheRestResult()
        {
            // Arrange
            // Invalid ssl certificate causes and error on the request
            var httpClientHandler = new HttpClientHandlerFakeWithFunc() { SendAsyncFunc = (r, c) => throw new HttpRequestException("An error occurred while sending the request.", new WebException("The underlying connection was closed: Could not establish trust relationship for the SSL/TLS secure channel.")) };
            Result<object> result;

            // Act
            using (var restClient = new RestClient("https://buildserver:8154", "username", "password", false, httpClientHandler))
            {
                result = restClient.Get<object>("/go/api/dashboard", "application/vnd.go.cd.v1+json");

            }

            // Assert
            Assert.That(result.IsValid, Is.False);
            Assert.That(result.ToString(), Is.EqualTo("An error occurred while sending the request. The underlying connection was closed: Could not establish trust relationship for the SSL/TLS secure channel."));
        }

        [Test]
        public void Get_IfTheResponseIsInvalid_TheMessageIsFormatted_AndReturnedInTheRestResult()
        {
            // Arrange
            // Invalid password causes Unauthorised in response
            var httpClientHandler = new HttpClientHandlerFakeWithFunc { SendAsyncFunc = (r, c) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.Unauthorized) { ReasonPhrase = "" })};
            Result<object> result;

            // Act
            using (var restClient = new RestClient("https://buildserver:8154", "username", "password", false, httpClientHandler))
            {
                result = restClient.Get<object>("/go/api/dashboard", "application/vnd.go.cd.v1+json");
            }

            //Assert
            Assert.That(result.IsValid, Is.False);
            Assert.That(result.ToString(), Is.EqualTo("Response status code does not indicate success: 401 (Unauthorized)."));
        }

        [Test]
        public void Get_SetsUpTheRequestCorrectly_AndDeserialisesTheResult()
        {
            // Arrange
            string url = string.Empty;
            string method = string.Empty;
            HttpHeaderValueCollection<MediaTypeWithQualityHeaderValue> acceptHeaders = null;
            AuthenticationHeaderValue authentication = null;

            var jsonResult = JsonConvert.SerializeObject(new TestObject() {Name = "to", Value = 2});
            var httpClientHandler = new HttpClientHandlerFakeWithFunc()
            {
                SendAsyncFunc = (r, c) =>
                {
                    url = r.RequestUri.ToString();
                    method = r.Method.ToString();
                    acceptHeaders = r.Headers.Accept;
                    authentication = r.Headers.Authorization;
                    return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) {Content = new StringContent(jsonResult)});
                }
            };
            Result<TestObject> result;

            // Act
            using (var restClient = new RestClient("https://buildserver:8154", "username", "password", false, httpClientHandler))
            {
                result = restClient.Get<TestObject>("/go/api/dashboard", "application/vnd.go.cd.v1+json");
            }

            //Assert
            Assert.That(result.IsValid, Is.True);
            Assert.That(url, Is.EqualTo("https://buildserver:8154/go/api/dashboard"));
            Assert.That(method, Is.EqualTo("GET"));
            Assert.That(acceptHeaders.Count, Is.EqualTo(1));
            Assert.That(acceptHeaders.First().ToString(), Is.EqualTo("application/vnd.go.cd.v1+json"));
            Assert.That(authentication.Scheme, Is.EqualTo("Basic"));
            Assert.That(authentication.Parameter, Is.EqualTo(Convert.ToBase64String(Encoding.ASCII.GetBytes($"username:password"))));
            Assert.That(result.Data.Name, Is.EqualTo("to"));
            Assert.That(result.Data.Value, Is.EqualTo(2));
        }
    }
}