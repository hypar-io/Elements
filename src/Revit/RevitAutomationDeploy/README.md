# Revit Automation Deployment Application
This application deploys Hypar's Model converter as an application for Revit Design Automation.

# Run
Several things need to be done in the right order to get this to work.
- Call `DELETE https://developer.api.autodesk.com/da/us-east/v3/forgeapps/me` to remove all data for our app.
- Call `PATCH https://developer.api.autodesk.com/da/us-east/v3/forgeapps/me` to create a nickname for our app with the following body:
```json
{
"nickname":"Hypar"
}
```
- Run the application like `dotnet run test` where `test` is the alias you want the application to have.