using System.Net;
using System.Threading.Tasks;
using Azure.DataGateway.Config;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Azure.DataGateway.Service.Tests.SqlTests.RestApiTests.Insert
{
    /// <summary>
    /// Test REST Apis validating expected results are obtained.
    /// </summary>
    [TestClass]
    public abstract class InsertApiTestBase : RestApiTestBase
    {
        #region Positive Tests
        /// <summary>
        /// Tests the InsertOne functionality with a REST POST request.
        /// </summary>
        [TestMethod]
        public virtual async Task InsertOneTest()
        {
            string requestBody = @"
            {
                ""title"": ""My New Book"",
                ""publisher_id"": 1234
            }";

            string expectedLocationHeader = $"id/{STARTING_ID_FOR_TEST_INSERTS}";
            await SetupAndRunRestApiTest(
                primaryKeyRoute: null,
                queryString: null,
                entity: _integrationEntityName,
                sqlQuery: GetQuery(nameof(InsertOneTest)),
                controller: _restController,
                operationType: Operation.Insert,
                requestBody: requestBody,
                expectedStatusCode: HttpStatusCode.Created,
                expectedLocationHeader: expectedLocationHeader
            );

            requestBody = @"
            {
                ""categoryid"": ""5"",
                ""pieceid"": ""2"",
                ""categoryName"":""FairyTales""
            }";

            expectedLocationHeader = $"categoryid/5/pieceid/2";
            await SetupAndRunRestApiTest(
                primaryKeyRoute: null,
                queryString: null,
                entity: _Composite_NonAutoGenPK_EntityName,
                sqlQuery: GetQuery("InsertOneInCompositeNonAutoGenPKTest"),
                controller: _restController,
                operationType: Operation.Insert,
                requestBody: requestBody,
                expectedStatusCode: HttpStatusCode.Created,
                expectedLocationHeader: expectedLocationHeader
            );
        }

        /// <summary>
        /// Tests the InsertOne functionality with a REST POST request
        /// where the entity has mapping defined for its columns.
        /// </summary>
        [TestMethod]
        public virtual async Task InsertOneWithMappingTest()
        {
            string requestBody = @"
            {
                ""treeId"" : 3,
                ""Scientific Name"": ""Cupressus Sempervirens"",
                ""United State's Region"": ""South East""
            }";

            string expectedLocationHeader = $"treeId/3";
            await SetupAndRunRestApiTest(
                primaryKeyRoute: null,
                queryString: null,
                entity: _integrationMappingEntity,
                sqlQuery: GetQuery(nameof(InsertOneWithMappingTest)),
                controller: _restController,
                operationType: Operation.Insert,
                requestBody: requestBody,
                expectedStatusCode: HttpStatusCode.Created,
                expectedLocationHeader: expectedLocationHeader
            );
        }

        /// <summary>
        /// Tests InsertOne into a table that has a composite primary key
        /// with a REST POST request.
        /// </summary>
        [TestMethod]
        public virtual async Task InsertOneInCompositeKeyTableTest()
        {
            string requestBody = @"
            {
                ""book_id"": ""1"",
                ""content"": ""Amazing book!""
            }";

            string expectedLocationHeader = $"book_id/1/id/{STARTING_ID_FOR_TEST_INSERTS}";
            await SetupAndRunRestApiTest(
                primaryKeyRoute: null,
                queryString: null,
                entity: _entityWithCompositePrimaryKey,
                sqlQuery: GetQuery(nameof(InsertOneInCompositeKeyTableTest)),
                controller: _restController,
                operationType: Operation.Insert,
                requestBody: requestBody,
                expectedStatusCode: HttpStatusCode.Created,
                expectedLocationHeader: expectedLocationHeader
            );

            requestBody = @"
            {
                 ""book_id"": ""2""
            }";

            expectedLocationHeader = $"book_id/2/id/{STARTING_ID_FOR_TEST_INSERTS + 1}";
            await SetupAndRunRestApiTest(
                primaryKeyRoute: null,
                queryString: null,
                entity: _entityWithCompositePrimaryKey,
                sqlQuery: GetQuery("InsertOneInDefaultTestTable"),
                controller: _restController,
                operationType: Operation.Insert,
                requestBody: requestBody,
                expectedStatusCode: HttpStatusCode.Created,
                expectedLocationHeader: expectedLocationHeader
            );
        }

        [TestMethod]
        public virtual async Task InsertOneWithNullFieldValue()
        {
            string requestBody = @"
            {
                ""categoryid"": ""3"",
                ""pieceid"": ""1"",
                ""piecesAvailable"": null,
                ""piecesRequired"": 1,
                ""categoryName"":""SciFi""
            }";

            string expectedLocationHeader = $"categoryid/3/pieceid/1";
            await SetupAndRunRestApiTest(
                primaryKeyRoute: null,
                queryString: null,
                entity: _Composite_NonAutoGenPK_EntityName,
                sqlQuery: GetQuery("InsertOneWithNullFieldValue"),
                controller: _restController,
                operationType: Operation.Insert,
                requestBody: requestBody,
                expectedStatusCode: HttpStatusCode.Created,
                expectedLocationHeader: expectedLocationHeader
            );
        }
        #endregion

        #region Negative Tests

        [TestMethod]
        public virtual async Task InsertOneWithInvalidQueryStringTest()
        {
            string requestBody = @"
            {
                ""title"": ""My New Book"",
                ""publisher_id"": 1234
            }";

            await SetupAndRunRestApiTest(
                primaryKeyRoute: string.Empty,
                queryString: "?/id/5001",
                entity: _integrationEntityName,
                sqlQuery: string.Empty,
                controller: _restController,
                operationType: Operation.Insert,
                requestBody: requestBody,
                exception: true,
                expectedErrorMessage: "Query string for POST requests is an invalid url.",
                expectedStatusCode: HttpStatusCode.BadRequest
            );
        }

        /// <summary>
        /// Tests the InsertOne functionality with disallowed request composition: array in request body.
        /// </summary>
        [TestMethod]
        public virtual async Task InsertOneWithInvalidArrayJsonBodyTest()
        {
            string requestBody = @"
            [{
                ""title"": ""My New Book"",
                ""publisher_id"": 1234
            }]";

            await SetupAndRunRestApiTest(
                primaryKeyRoute: string.Empty,
                queryString: string.Empty,
                entity: _integrationEntityName,
                sqlQuery: string.Empty,
                controller: _restController,
                operationType: Operation.Insert,
                requestBody: requestBody,
                exception: true,
                expectedErrorMessage: "Mutation operation on many instances of an entity in a single request are not yet supported.",
                expectedStatusCode: HttpStatusCode.BadRequest,
                expectedSubStatusCode: "BadRequest"
            );
        }

        /// <summary>
        /// Tests the InsertOne functionality with a request body containing values that do not match the value type defined in the schema.
        /// </summary>
        [TestMethod]
        public virtual async Task InsertOneWithInvalidTypeInJsonBodyTest()
        {
            string requestBody = @"
            {
                ""title"": [""My New Book"", ""Another new Book"", {""author"": ""unknown""}],
                ""publisher_id"": [1234, 4321]
            }";

            await SetupAndRunRestApiTest(
                primaryKeyRoute: string.Empty,
                queryString: string.Empty,
                entity: _integrationEntityName,
                sqlQuery: string.Empty,
                controller: _restController,
                operationType: Operation.Insert,
                requestBody: requestBody,
                exception: true,
                expectedErrorMessage: "Parameter \"[1234, 4321]\" cannot be resolved as column \"publisher_id\" with type \"Int32\".",
                expectedStatusCode: HttpStatusCode.BadRequest,
                expectedSubStatusCode: "BadRequest"
            );
        }

        /// <summary>
        /// Tests the InsertOne functionality with no valid fields in the request body.
        /// </summary>
        [TestMethod]
        public virtual async Task InsertOneWithNoValidFieldInJsonBodyTest()
        {
            string requestBody = @"
            {}";

            await SetupAndRunRestApiTest(
                primaryKeyRoute: string.Empty,
                queryString: string.Empty,
                entity: _integrationEntityName,
                sqlQuery: string.Empty,
                controller: _restController,
                operationType: Operation.Insert,
                requestBody: requestBody,
                exception: true,
                expectedErrorMessage: "Invalid request body. Missing field in body: title.",
                expectedStatusCode: HttpStatusCode.BadRequest
            );
        }

        /// <summary>
        /// Tests the InsertOne functionality with an invalid field in the request body:
        /// Primary Key in the request body for table with Autogenerated PK.
        /// </summary>
        [TestMethod]
        public virtual async Task InsertOneWithAutoGeneratedPrimaryKeyInJsonBodyTest()
        {
            string requestBody = @"
            {
                ""id"": " + STARTING_ID_FOR_TEST_INSERTS +
            "}";

            await SetupAndRunRestApiTest(
                primaryKeyRoute: string.Empty,
                queryString: string.Empty,
                entity: _integrationEntityName,
                sqlQuery: string.Empty,
                controller: _restController,
                operationType: Operation.Insert,
                requestBody: requestBody,
                exception: true,
                expectedErrorMessage: "Invalid request body. Field not allowed in body: id.",
                expectedStatusCode: HttpStatusCode.BadRequest
            );
        }

        /// <summary>
        /// Tests the InsertOne functionality with a missing field from the request body:
        /// Missing non auto generated Primary Key in Json Body.
        /// </summary>
        [TestMethod]
        public virtual async Task InsertOneWithNonAutoGeneratedPrimaryKeyMissingInJsonBodyTest()
        {
            string requestBody = @"
            {
                ""title"": ""My New Book""
            }";

            await SetupAndRunRestApiTest(
                primaryKeyRoute: string.Empty,
                queryString: string.Empty,
                entity: _integration_NonAutoGenPK_EntityName,
                sqlQuery: string.Empty,
                controller: _restController,
                operationType: Operation.Insert,
                requestBody: requestBody,
                exception: true,
                expectedErrorMessage: "Invalid request body. Missing field in body: id.",
                expectedStatusCode: HttpStatusCode.BadRequest,
                expectedSubStatusCode: "BadRequest"
            );
        }

        /// <summary>
        /// Tests the InsertOne functionality with a missing field in the request body:
        /// A non-nullable field in the Json Body is missing.
        /// </summary>
        [TestMethod]
        public virtual async Task InsertOneWithNonNullableFieldMissingInJsonBodyTest()
        {
            string requestBody = @"
            {
                ""id"": " + STARTING_ID_FOR_TEST_INSERTS + ",\n" +
                "\"issue_number\": 1234\n" +
            "}";

            await SetupAndRunRestApiTest(
                primaryKeyRoute: string.Empty,
                queryString: string.Empty,
                entity: _integration_NonAutoGenPK_EntityName,
                sqlQuery: string.Empty,
                controller: _restController,
                operationType: Operation.Insert,
                requestBody: requestBody,
                exception: true,
                expectedErrorMessage: "Invalid request body. Missing field in body: title.",
                expectedStatusCode: HttpStatusCode.BadRequest,
                expectedSubStatusCode: "BadRequest"
            );

            requestBody = @"
            {
                ""categoryid"":""6"",
                ""pieceid"":""1""
            }";

            await SetupAndRunRestApiTest(
                primaryKeyRoute: null,
                queryString: string.Empty,
                entity: _Composite_NonAutoGenPK_EntityName,
                sqlQuery: string.Empty,
                controller: _restController,
                operationType: Operation.Insert,
                requestBody: requestBody,
                exception: true,
                expectedErrorMessage: "Invalid request body. Missing field in body: categoryName.",
                expectedStatusCode: HttpStatusCode.BadRequest,
                expectedSubStatusCode: "BadRequest"
            );
        }

        [TestMethod]
        public virtual async Task InsertOneWithNonNullableFieldAsNull()
        {
            string requestBody = @"
            {
                ""categoryid"": ""3"",
                ""pieceid"": ""1"",
                ""piecesAvailable"": 1,
                ""piecesRequired"": null,
                ""categoryName"":""Fantasy""
            }";

            await SetupAndRunRestApiTest(
                primaryKeyRoute: string.Empty,
                queryString: string.Empty,
                entity: _Composite_NonAutoGenPK_EntityName,
                sqlQuery: string.Empty,
                controller: _restController,
                operationType: Operation.Insert,
                requestBody: requestBody,
                exception: true,
                expectedErrorMessage: "Invalid value for field piecesRequired in request body.",
                expectedStatusCode: HttpStatusCode.BadRequest
            );

            requestBody = @"
            {
                ""categoryid"": ""3"",
                ""pieceid"": ""1"",
                ""piecesAvailable"": 1,
                ""piecesRequired"": 1,
                ""categoryName"":null
            }";

            await SetupAndRunRestApiTest(
                primaryKeyRoute: string.Empty,
                queryString: string.Empty,
                entity: _Composite_NonAutoGenPK_EntityName,
                sqlQuery: string.Empty,
                controller: _restController,
                operationType: Operation.Insert,
                requestBody: requestBody,
                exception: true,
                expectedErrorMessage: "Invalid value for field categoryName in request body.",
                expectedStatusCode: HttpStatusCode.BadRequest
            );
        }

        /// <summary>
        /// Verifies that we throw exception when field
        /// provided to insert is an exposed name that
        /// maps to a backing column name that does not
        /// exist in the table.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task InsertTestWithInvalidMapping()
        {
            string requestBody = @"
            {
                ""speciesid"" : 3,
                ""hazards"": ""black mold"",
                ""region"": ""Pacific North West""
            }";

            string expectedLocationHeader = $"speciedid/3";
            await SetupAndRunRestApiTest(
                primaryKeyRoute: string.Empty,
                queryString: string.Empty,
                entity: _integrationBrokenMappingEntity,
                sqlQuery: string.Empty,
                controller: _restController,
                operationType: Operation.Insert,
                exception: true,
                requestBody: requestBody,
                expectedErrorMessage: "Invalid request body. Contained unexpected fields in body: hazards",
                expectedStatusCode: HttpStatusCode.BadRequest,
                expectedSubStatusCode: "BadRequest",
                expectedLocationHeader: expectedLocationHeader
                );
        }

        #endregion
    }
}