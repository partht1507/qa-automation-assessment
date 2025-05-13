using RestSharp;

namespace ApiAutomationSolution
{
    // Simple dto for Petstore pet object
    public class Pet
    {
        public long Id { get; set; }                // pet ID
        public required string Name { get; set; }            // pet name
        public required string[] PhotoUrls { get; set; }     // list of pics
    }

    [TestFixture]
    public class ApiAutomationTests
    {
        private RestClient client;
        private long petId;

        [SetUp]
        public void SetUp()
        {
            // initialize RestSharp client for the Petstore demo API
            client = new RestClient("https://petstore.swagger.io/v2");
            // pick a random ID to avoid collisions
            petId = new Random().Next(10000, 99999);
        }

        [TearDown]
        public void TearDown()
        {
            // clean up the client when we're done
            client?.Dispose();
        }

        // helper: send request and log basic info
        private async Task<RestResponse<T>> Send<T>(RestRequest req)
        {
            Console.WriteLine($"--> {req.Method} {req.Resource}");
            var res = await client.ExecuteAsync<T>(req);
            Console.WriteLine($"<-- {(int)res.StatusCode} {res.StatusDescription}");
            Console.WriteLine(res.Content);
            return res;
        }

        // Uploads a file using a simple Bearer token auth pattern
        public async Task UploadFileAsync(string filePath, string token = "fake-jwt-token")
        {
            var rest = new RestClient("https://httpbin.org");
            var req = new RestRequest("/post", Method.Post)
                .AddHeader("Authorization", $"Bearer {token}");

            // read the file so we can attach its bytes
            var data = await File.ReadAllBytesAsync(filePath);
            req.AddFile("file", data, Path.GetFileName(filePath), "application/octet-stream");

            var res = await rest.ExecuteAsync(req);
            Assert.That(res.IsSuccessful, Is.True, $"Upload failed: {res.ErrorMessage}");
        }

        [Test, Category("API")]
        public async Task ChainApiCalls()
        {
            // 1) CREATE a new pet
            var createReq = new RestRequest("/pet", Method.Post)
                .AddJsonBody(new Pet { Id = petId, Name = "Fluffy", PhotoUrls = new[] { "http://example.com/fluffy.jpg" } });
            var createRes = await Send<Pet>(createReq);
            Assert.That(createRes.IsSuccessful, Is.True, "Oops, failed to create pet");

            // 2) UPLOAD an image for the newly created pet
            var imgPath = Path.Combine(TestContext.CurrentContext.TestDirectory, "Resources", "fluffy.png");
            var upReq = new RestRequest($"/pet/{petId}/uploadImage", Method.Post) { AlwaysMultipartFormData = true };
            upReq.AddFile("file", imgPath, "image/png");
            var upRes = await Send<object>(upReq);
            Assert.That(upRes.IsSuccessful, Is.True, "Image upload didn't work");

            // 3) DELETE the pet to clean up
            var delReq = new RestRequest($"/pet/{petId}", Method.Delete);
            var delRes = await Send<object>(delReq);
            Assert.That(delRes.IsSuccessful, Is.True, "Couldn't delete the pet");

            // 4) VERIFY deletion by trying to GET it (should be 404)
            var getReq = new RestRequest($"/pet/{petId}", Method.Get);
            var getRes = await Send<Pet>(getReq);
            Assert.That((int)getRes.StatusCode, Is.EqualTo(404), "Pet should be gone at this point");
        }

        [Test, Category("API")]
        public async Task UploadFileAsync_Test()
        {
            // make sure our sample file is in place
            var file = Path.Combine(TestContext.CurrentContext.TestDirectory, "Resources", "sample.txt");
            Assert.That(File.Exists(file), Is.True, "sample.txt missing");

            // reuse the helper for file uploads
            await UploadFileAsync(file);
        }

        
    }
}
