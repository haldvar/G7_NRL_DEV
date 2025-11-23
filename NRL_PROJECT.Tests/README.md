How to run tests
- From the solution root:
  dotnet test ./NRL_PROJECT.Tests/NRL_PROJECT.Tests.csproj

What is covered
- Model behavior: MapData.CoordinateSummary (Point, LineString, empty)
- ObstacleData initialization and NotMapped attribute presence
- In-memory EF Core checks: saving and loading MapData with coordinates and ObstacleReport defaults

Notes about hard-to-test areas
- Controllers that depend on ASP.NET services: many controllers use UserManager<User>, SignInManager, IWebHostEnvironment, HttpContext and model binding. Those require more setup:
  - Use WebApplicationFactory<TStartup> (Microsoft.AspNetCore.Mvc.Testing) for integration tests, or
  - Mock UserManager/SignInManager with Moq and create ControllerContext with a fake HttpContext.
- File upload flows (IFormFile) interact with IWebHostEnvironment and the filesystem. For unit tests prefer:
  - Use in-memory streams for IFormFile and mock IWebHostEnvironment.WebRootPath, and clean up temporary files.
- DataSeeder.SeedAsync and direct DB integration: test with InMemory provider or a disposable test container (Testcontainers / Docker) for true MySQL behavior.
- JS-heavy behavior (map rendering, client interactions): test manually or with end-to-end tests (Playwright, Selenium).

Next steps (optional)
- Add integration tests for one controller using WebApplicationFactory to validate full request/response behavior.
- Add controller unit tests by mocking UserManager and SignInManager if you want isolated tests of controller logic.