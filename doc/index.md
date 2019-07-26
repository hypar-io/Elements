# Getting Started with Hypar

Hypar is a cloud platform for generating buildings that makes it easy to publish, distribute, and maintain your building design logic. Rather than requiring web development skills to scale and deliver your design logic, Hypar lets you concentrate on what you want to get done while the platform creates the interface for your audience and provides computation, visualization, delivery, interoperability, and access control for your functions.

By uploading a function to Hypar you can produce twenty options for a building problem in just a few seconds:

![](./images/HyparIntro.png)

In this introduction to Hypar, we'll work with a much simpler function that generates masses of variable dimensions.

<div style="page-break-after: always;"></div>

## Pre-Flight Check
Before getting started, there are a few things you'll need and some other things you might want to know.

### Things you'll need
* A 'modern' web browser. 
    * We'll use **Google Chrome** for this guide, but **Firefox** or **Brave** should also work fine.
        * **Chrome**: https://www.google.com/chrome/
        * **Firefox**: https://www.mozilla.org/en-US/firefox/new/
        * **Brave**: https://brave.com/download/
    
* **Visual Studio Community** (Windows) or **VS Code** (Windows, Mac OS, Linux)
    * We'll use **Visual Studio Community** or **VSC** for this guide, but if you're more comfortable in **VS Code**, take analogous actions in that editor when we come to those steps.
        * **Visual Studio Community**: https://visualstudio.microsoft.com/vs/community/
    
* Access to a command line for your operating system.
    * We'll use the **Windows Command Prompt** for this guide, but other operating system command prompts should work similarly.
    
* The Microsoft .Net Core 2.1. 
    * Hypar uses the cross-platform dotnet framework created and maintained by Microsoft. The version number is important! There might be issues with later or earlier versions of .Net Core.
        * **.Net Core 2.1**: https://dotnet.microsoft.com/download/dotnet-core/2.1
    
### Things you might want to know
* Basic familiarity with the **C#** programming language will help, but if you're not familiar with C# we'll do our best to walk you through making changes to the initial code you'll get by following the steps in this guide.
* If you want to share your Hypar functions, you have to explicitly make your work public, so don't worry that perfect strangers are judging your work. They will, but only if **<u>you</u>** decide to make them public.
* The procedures you'll use in this guide compile your code on your desktop and only upload the resulting binary file. No one at Hypar will examine your source code because we won't have it unless you make it public by some other means or if you send it to us so we can help you solve a problem.
* None of the Hypar libraries you'll use in this guide bind your code to Hypar web services. For example, if you'd like to use the Elements library for a desktop application, it's an [open source project on GitHub](https://github.com/hypar-io/Elements) and will always be free for anyone to use or extend. You don't even have to tell us you're using it.

<div style="page-break-after: always;"></div>

### Signing up for Hypar

To upload anything to the Hypar platform, you're going to need an account. 
Point your browser to <a href="https://hypar.io" target="_blank">https://hypar.io</a>, and you should see a page that looks like this. 

![](./images/HyparLanding2019.06.16.png)

<div style="page-break-after: always;"></div>

If you don't have an account already, click on the **Sign Up** button below the logo on the right. That should take you to the Sign Up screen.

![](./images/HyparLogin2019.06.16.png)

<div style="page-break-after: always;"></div>

Fill in a username, an email address you can access, and the password you'd like to use. Then click on the **Sign up** button in the center of the screen.

![](./images/HyparSignUp2019.06.16.png)

<div style="page-break-after: always;"></div>

Then we do some explaining:

![](./images/HyparSignUpNotice2019.06.16.png)

Now check the email account you supplied for a message that looks something like this:

![](./images/HyparSignUpEmail2019.06.16.png)

<div style="page-break-after: always;"></div>

Click on the <u>Verify Email</u> link in the message and you should see a confirmation page:

![](./images/HyparSignUpConfirm2019.06.16.png)

That's it! You now have a Hypar account. Later in this guide you'll sign in to see your first function in Hypar Explore.

<div style="page-break-after: always;"></div>

### Installing and Using the Hypar Command Line Interface (CLI)

Open a Windows Command Prompt and input the following command:

```bash
dotnet tool install -g hypar.cli
```
![](./images/HyparCLIinstall2019.06.16.PNG)

Now you're ready to send your first function to Hypar. First use the command prompt cd (for "<u>c</u>hange <u>d</u>irectory") command to navigate to the folder where you'd like to place your function project.

Now try this in the same Windows Command Prompt:
```bash
hypar new
```
![](./images/HyparFunctionName2019.06.16.png)

<div style="page-break-after: always;"></div>

For consistency with the rest of this example give your function the name **StarterFunction**. A bunch of stuff happens that we'll explain in a moment, but in the meantime enter your Hypar user name and password:

![](./images/HyparFunctionNewLogin2019.06.16.png)

<div style="page-break-after: always;"></div>

Now more stuff happens, but the important thing right now is to know that your function has been published (privately) to Hypar! 

![](./images/HyparFunctionPublished2019.06.16.png)

With one command you've created a web application that we're going to customize and publish again (and again and again), but first let's see what we've got so far. <a href="Back to https://hypar.io" target="_blank">Back to https://hypar.io</a>.

<div style="page-break-after: always;"></div>

This time, click on the **Sign In** button on the left below the logo...

![](./images/HyparLanding2019.06.16.png)

...and sign in using your new account:

![](./images/HyparSignIn2019.06.16.png)

<div style="page-break-after: always;"></div>

Once you sign in, you'll see something like the following screen:

![](./images/HyparStarterFunction2019.06.16.PNG)

<div style="page-break-after: always;"></div>

All the function cards have default graphics because they haven't produced options yet. Click on the card flipper arrow on the lower right of your **StarterFunction** card:

![](./images/HyparCardFlip2019.06.16.png)

That checked **Private** box is what's keeping your new function invisible to everyone but you and the administrators of Hypar. If you ever want everyone to see your function, you'll have to uncheck that box and read our warning that the world is about to see your work. Try changing the setting, then make it private again.

<div style="page-break-after: always;"></div>

Now let's make some options. Click on **StarterFunction** card title. You should see something like this:

![](./images/HyparStarterFunctionNoOptions2019.06.16.PNG)

Click on the **Sample 10 Options** in the lower left corner of the page to generate some options. After a few progress messages go by, you should see something like this:

![](./images/HyparOptions2019.06.16.png)

<div style="page-break-after: always;"></div>

You've generated a bunch of extruded rectangles! More importantly, you've just used a web application you created with one command. Let's do a little more exploration into the options before we explain everything that happened. Click on one of the option cards to look at one of them in more detail:

![](./images/HyparOption2019.06.16.png)

Click in the geometry display window to zoom, pan, and rotate the option.

<div style="page-break-after: always;"></div>

Now let's go back and look at everything that happened and what it means for writing your own custom Hypar functions in the future. We'll place blue boxes around what we're talking about, like this:

![](./images/HyparStartFunctionCreate2019.06.16.png)

The first thing that happened was that the **hypar new** command created a new local project folder within the current folder. In Windows Explorer, the new folder looks like this:

![](./images/HyparStarterFolder2019.06.16.png)

The next thing **hypar new** did was add a test project into that folder, in the folder called **test** above. This is the project that will help you test your function updates locally before uploading them to Hypar.

![](./images/HyparStartFunctionTests2019.06.16.PNG)

<div style="page-break-after: always;"></div>

Then the **hypar new** command created a **hypar.json** file that you can see in the **StarterFunction** folder above. We'll use this file to customize and extend the default function.

![](./images/HyparJSONCreate019.06.16.png)

Next the **hypar new** command executed something called **hypar init**, which created a couple of more important files in your project, but which you shouldn't have to worry about except to understand what they do and how they change as you edit your **hypar.json** file.

![](./images/HyparInit2019.06.16.png)

<div style="page-break-after: always;"></div>

If you open the **src** folder, you'll see these files:

![](./images/HyparSRC2019.06.16.png)

The three files with names containing **.g.cs** are generated code files. You never need to edit these directly but it's important to know that the **hypar init** command generates them from the **hypar.json** file that we'll look at in a moment, because when you make changes to the **hypar.json** file you'll have to run **hypar init** again to update these files. 

These three files are what make your C# code compatible with Hypar services. They're kept separate so that your code won't become directly dependent on the Hypar platform but can easily take advantage of its services.

After **hypar init** executes, there's some housekeeping that completes the local changes, then after you sign in the **hypar new** command uploads your new function.

In the **StarterFunction** folder you'll see a matching **StarterFunction.sln** file.

![](./images/HyparStarterFolder2019.06.16.png)

<div style="page-break-after: always;"></div>

Double click on **StarterFunction.sln** to open it in Visual Studio Community (VSC).

![](./images/HyparSLN2019.06.16.png)

Now use VSC to open the **hypar.json** file, and we'll look at how this configuration influences what you see when you work with a function on Hypar. 

**IMPORTANT:**
**Do <u>not</u> use "Add Existing Item" to add the hypar.json to the project. VSC will silently make a copy and place it in the ./src folder, while the Hypar CLI will still read the original file. Just open the file to edit it when you need to.**

The **"inputs"** section of the **hypar.json** determines what inputs you see for the uploaded function:

![](./images/HyparInputs2019.06.16.png)

The **"outputs"** section determines the values you see associated with each option:

![](./images/HyparOutputs2019.06.16.png)

In the next exercise, we're going add a new **Height** input to this function.

<div style="page-break-after: always;"></div>

### Adding a new function input

First, let's open the **StarterFunctionInputs.g.cs** file in VSC and look at the **StarterFunctionInputs** class to see how the **hypar.json** turns into code. Note how the corresponding entries become public class properties:

![](./images/HyparInputsClassJSON2019.06.16.png)

<div style="page-break-after: always;"></div>

Edit your **hypar.json** file to look like the one illustrated below. Copy the **"Width"** output and change the copy's **name** and **description** values to refer to **"Height"** and **"The height"** instead of **"Width"** and **"The width"**. VSC politely inserts a comma after the **"Width"** section's closing brace now that it's no longer the last input field:

![](./images/HyparHeightInputJSON2019.06.16.png)

Save this file and open the command prompt again, using the cd command to change the current directory to your project folder. Then run **hypar init**:

![](./images/HyparInitCommand2019.06.16.png)

**hypar init** reads the json file to understand how to regenerate the input and output class files as well as the function file. When you return to VSC, if the **StarterFunctionInputs.g.cs** file is still open, you'll see a warning that the file has been changed externally, which is what **hypar init** did when it rewrote the three **.g.cs** files. Select "Yes to All" if you get this warning, and open **StarterFunctionInputs.g.cs** to see what changed.

<div style="page-break-after: always;"></div>

![](./images/HyparInitChange2019.06.16.png)

Now that we have this new entry for **Height**, we have to tell our function to use it. Open **StarterFunction.cs** and change the **var height = 1.0;** line to use the height input value instead:

![](./images/HyparHeightInput2019.06.16.png)

<div style="page-break-after: always;"></div>

Now we have our new **Height** input. Let's send this up to Hypar using **hypar publish** and see how our function works now:

![](./images/HyparPublish2019.06.16.png)

Opening **StarterFunction** on Hypar again, we now have a **Height** input in addition to the **Length** and **Width** inputs. Setting height to 10 gives us 10 options with identical heights and varied lengths and widths:

![](./images/HyparStarterHeight2019.06.16.png)

<div style="page-break-after: always;"></div>

What if we wanted a maximum height of 20? Let's go back to the **hypar.json** file and change the maximum for the input range:

![](./images/HyparStarter20Height2019.06.16.png)

Save the file and run **hypar publish** again at the command prompt:

![](./images/HyparPublish2019.06.16.png)

<div style="page-break-after: always;"></div>

When we open **StarterFunction** on Hypar again, the range is changed:

![](./images/HyparHeightUpdate.png)

If you click the **Sample 10 Options** button with the height set to 20, you should see results that look something like this:

![](./images/HyparHeight20Options.png)

<div style="page-break-after: always;"></div>

Now let's allow Hypar to sample the **Height** range instead by toggling **Select Value** to **Sample Range** on the slider:

![](./images/HyparSample20Options.png)

Note that **Sample 10 Options** has become **Sample 20 Options**. What happened there?

<div style="page-break-after: always;"></div>

Hypar uses the widest range of values within a group of sliders to determine how many options it will create at once, up to a limit of 20. Since your **Height** slider now has a **min** of **1** and a **max** of **20** with a **step** of **1**, you now can sample 20 options:

![](./images/HyparJSONHeightMinMaxStep.PNG)

<div style="page-break-after: always;"></div>

### Adding a new function output

In Hypar click one of the options and look at the function outputs. You should see something like this, although your volume result might be different:

![](./images/HyparOutputs.png)

<div style="page-break-after: always;"></div>

What if we also wanted to display the area of our cube? For this exercise we'll add a new output to the **hypar.json** and calculate the result we need. Add these new lines to the **hypar.json**:

![](./images/HyparJSONOutputs.PNG)

<div style="page-break-after: always;"></div>

Copy the **"Volume"** section down and change its **"name"** field to **"Area"** and its **"description"** to **"The area"**. After those changes, on the command line in the project folder run **hypar init**:

![](./images/HyparInitCommand2019.06.16.png)

Now in VSC let's see what changed. Open **StarterFunctionOutputs.g.cs** and you should see your new **Area** output under the **Volume** output:

![](./images/HyparOutputsChanged.PNG)

Also note that the **StarterFunctionOutputs** constructor at the bottom of the illustration has a new **double area** argument. 

<div style="page-break-after: always;"></div>

That new argument is important when we open **StarterFunction.cs** in VSC and see that we now have an error condition:

![](./images/HyparStarterFunctionError.png)

That's because we're still calling **StarterFunctionOutputs** with just the **volume** output. We have to calculate and add the **area** output like this:

![](./images/HyparAreaOutput.PNG)

<div style="page-break-after: always;"></div>

Once you've made that change and saved the file, publish the function to Hypar again:

![](./images/HyparPublish2019.06.16.png)

In Hypar return to the **StarterFunction** options page either by clicking on the browser's **back** button or by clicking on **explore** and the **StarterFunction** card again. Then click on the **Sample 20 Options** button to arrive at a screen that looks something like this:

![](./images/HyparAreaOptions.png)

<div style="page-break-after: always;"></div>

Click on one of the options. Now you should see your area calculation on the left side:

![](./images/HyparAreaResult.png)

What if we wanted to see these results in the graphic display as well?
We'll handle that by adding **properties** to the generated masses, and display them using the **Label** dropdown. For the moment the **Label** dropdown only has **None** as an entry:

![](./images/HyparLabels.png)

<div style="page-break-after: always;"></div>

To add labels for our **volume** and **area** values open **StarterFunction.cs** in VSC. Move the two value calculations up under the **height** input, because we're going to need those values a little earlier so we can add them as properties to our **mass**:

![](./images/HyparStartFunctionLabels1.PNG)

<div style="page-break-after: always;"></div>

Before we add the **mass** to our Hypar **model**, we need to add a couple of **NumericProperty** entries to the **mass**. Add these lines before **model.AddElement(mass)**:

![](./images/HyparAddProperties.PNG)

There's a lot happening in these two lines, so let's look at each part, using our new **Volume** property as an example of how both lines work. Since we're adding a property to our **mass**, we use its method **AddProperty**:

![](./images/HyparAddProperty.PNG)

The first argument this method needs is a name for the new property, which in this case is **"Volume"**:

![](./images/HyparAddPropertyName.PNG)

Next the method needs to know what kind of property you're going to add. Eligible values for this argument are **StringProperty** (if we were adding a string value), or in this case **NumericProperty**:

![](./images/HyparAddNumericProperty.PNG)

<div style="page-break-after: always;"></div>

The new **NumericProperty** needs two arguments: the value, which in this case is the **volume** variable calculated above, and then the **UnitType**, which for this output is **UnitType.Volume**:

![](./images/HyparNewNumericProperty.PNG)

Save the file and publish **StarterFunction** again:

![](./images/HyparPublish2019.06.16.png)

<div style="page-break-after: always;"></div>

When you return to Hypar, click on the **Sample 20 Options** button in **StarterFunction** to create 20 new options, then click on one of the options to open it in the interactive display. In the **Label** dropdown you should see the two new properties you just added:

![](./images/HyparLabels2.PNG)

Select **volume** and you should see the value appear in the graphic display on the mass:

![](./images/HyparLabels3.png)

Now you see why we specified the **UnitType** for the new property. Specifying the **UnitType** as **UnitType.Volume** tells Hypar to add **m3** to the displayed value to denote cubic meters. Internally Hypar maintains all values in metric units. Functions can calculate the conversion to Imperial units if required. 

<div style="page-break-after: always;"></div>

If you select **area** in the **Label** dropdown, you should see the value notated in square meters:

![](./images/HyparLabels4.png)

You're not limited to simple values like these. You can add any static or calculated value you'd like to any Hypar Element (like Mass). 

To learn more about the open source Hypar Elements library browse to the [GitHub repository](https://github.com/hypar-io/Elements).

