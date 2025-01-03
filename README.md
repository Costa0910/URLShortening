# Backend

The backend of the URLify service, built with ASP.NET Core. It utilizes Entity Framework Core for database management and Identity for secure user authentication.

For full functionality, integrate this backend with the [Frontend Repository](https://github.com/Costa0910/url-shortening-frontend).

## Setup Instructions

1. **Clone the repository:**
    ```bash
    git clone https://github.com/Costa0910/URLShortening
    ```

2. **Navigate to the project directory:**
    ```bash
    cd URLShortening
    ```

3. **Restore dependencies:**
    ```bash
    dotnet restore
    ```

4. **Configure the application:**
    Copy `appsettings.example.json` to `appsettings.json` and edit it to include your database connection string and other required settings.

5. **Apply database migrations:**
    ```bash
    dotnet ef database update
    ```

6. **Run the application:**
    ```bash
    dotnet run
    ```

7. **Access the API:**
    After starting the application, use the Swagger interface, which opens automatically in your browser, to explore and test the API endpoints.