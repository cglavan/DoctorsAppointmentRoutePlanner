# My understanding
I was tasked with creating a simple web application with the following requirements:

**Scenario**

A doctor makes house calls. Given a set of appointments for a single day, build a route planner that produces the most efficient schedule. The doctor starts and ends at home, must respect appointment time windows, and may wait if early.

**Technical Requirements:**

*   Framework: ASP.NET Core 8+    
*   UI: Razor Pages    
*   Language: C#    
*   Input: JSON uploaded or pasted, containing:    
    *   Patient ID        
    *   Patient name        
    *   Patient latitude        
    *   Patient longitude        
    *   Time window (start and end)        
    *   Duration        
*   Output: Ordered route and schedule    
*   Backend: No database    
*   No external routing API required (if used, it must be behind an interface with a local fallback)

# Assumptions
*   User authentication was not included in the specifications, however I implemented a basic username/password screen to provide such a mechanism.  While not robust, it demonstrates my thought process for at least providing basic security for an application such as this.  In a production enviroment I would implement a more suitable and secure architecture (i.e. Azure RBAC, JWT token authentication, etc).
*   In addition to authentication I implemented an encryption algorithm that I have used in the past to encrypt the username/password (since they are stored in the appsettings.json file).  The values used in the encryption library are also stored in appsettings, however a more secure approach would be to store them in Azure Key Vault or another secrets platform.
*   A pre-formatted JSON input was not provided, so based on the requirements I constructed a basic JSON format for the appointment data to be used.  I have included a **SampleData.json** file in my solution for you to use during testing.
*   With the open-ended concept in mind I added a map component to visually display the generated route.  It uses the LeafLet map objects and simply takes a list of coordinates to generate the route and markers.  While this does require an active internet connection Leaflet does have the option of implementing an offline mode, although it requires downloading a considerably large library of map tiles.
*   To meet the "no external routing API" requirement I implemented a LocalRoutePlanner class and used the Haversine formula to calculate distance points.  The application can be extended quite easily to switch between an external API and local routing options.
*   In the interest of providng a near-complete application, I also added some additional features that I saw useful to the overall concept of the application.  Those features include:   
    *   The ability to add notes to an appointment for the doctor to review prior to meeting with the patient        
    *   The ability for the doctor to add post-appointment notes
    *   The ability for the doctor to specifiy if the appointment was completed or not
    *   Visual indictions as to the status of the appointment

# Implementation
The application  is built using ASP.NET Core and .Net 8. It includes the following structure:

Folder Structure:
*   Common: Includes the Encryption class for encrypting/decrypting data
*   Interfaces: Includes the **IRoutePlanner** interface used by the Index.cshtml and LocalRoutePlanner class
*   Models: Includes the data structures used throughout the application.
*   Services: Contains the logic classes for route planning and authentication.

Additionally, the project includes the following features:
- Serilog: Handles application logging.
- Dependency Injection: Manages service lifetimes and dependencies.

# Usage

The application can be run locally using the .NET CLI or Visual Studio. Once running, the application can be accessed by navigating to [https://localhost:7022](https://localhost:7022/). 

To log into the application the username and password are as follows:
*   **Username**: doctor
*   **Password**: P@ssw0rd!

Upon logging in you will need to paste the JSON text found in the **SampleData.json** file into the provided textarea and click the "Plan route" button.  Upon doing so the UI will update with the list of appointments and update the map to provide a visual representation of the calculated route.  You will see that the route includes various markers for each appointment, as well as directional arrows to quickly see the route path.

By clicking any of the markers on the map you will see a popup with basic information about the appointment, including the route stop number, patient name and the arival/departure times.

Each entry on the appointments list contains pre-visit notes (as determined by the JSON input), checkboxes for marking if the appointment was completed and a button to add notes pertaining to the specific appointment.  Once a note is saved you will have the ability to export the notes to a JSON file to use later.

Visual cues have been added to easily determine if an appointment was completed or not based on the checkboxes (i.e. left border of the appointment will turn green or red)

The route summary module displays basic information regarding the caluclated route.  It includes the total distance and time calculated, as well as the ability to switch between mile and kilometers for the display.
