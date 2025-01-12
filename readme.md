# Listify.Me.

Source code repo for listify.me, a rendering engine to display, index and support the use of lists of activities using .md formatted files to describe the activities.  



## Value Motivation

To create a useful web based solution that will help improve learning of specific topics while delivering a usable and useful application.   Specifically to learn functional programming, open telemetry 
and reinvigorate the understanding of active tracing for applications.  This will be done as an end to end activity trying to give an instance of each of the practices for agile delivery.

Deliver a usable list management system as a part of the learning but that is secoondary to the learning objective.



### Releases.

### Angel.  👼

Angel release is the first release, aimed at getting the CI/CD pipelines up and running and getting some basic deployment working with a single unit test and a single integration test.


#### Future Releases.

* Bealzebub. 😈
* Church. ⛪
* Fairy.🧚
* Ghost. 👻
* JackOLantern. 🎃
* Lizard. 🦎
* Ogre. 👹



### Getting Started.



#### PreReqs

Visual Studio 2022, .net 9    
Jira Board Access [here](https://plisky.atlassian.net/jira/software/projects/LFY)   
nuke global tool [documentation here](https://nuke.build/docs/getting-started/installation)]    

#### Steps.

Clone repository.
Open solution \sln\listify.sln.
Set "Listify" as startup project and run.( VS 2022 Enterprise tested).

🎵 The solution sets listify.build to not compile in the default configuration, therefore to make changes to and compile the build engine you must right click on it and compile it directly.
🎵 The first time through for a playwrite test you need to execute playwright.ps1 in the build directory to get the browser drivers down.

### Build and CI.

Build using nuke.
Standard primary steps are ArrangeStep, ConstructStep, ExamineStep, PackageStep, ReleaseStep, TestStep.

Each is incremental and builds on the previous step.  Arrange - initialise, Construct Build, Examine Test and Quality Check, Package - create a deployable package, Release - deploy the package.
Therefore to build and test the project run "nuke Examine" from the \src folder this is the fastest form of feedback loop.

CI is configured for the trunk branch.


### Environments & Configuration
1100 - [ListifyConfig1100] - Local Dev Environment ( Per Machine )  
1101 - [ListifyConfig1101] - Integrated development environment.   http://saspitsey-001-site4.dtempurl.com/

Environment configuration is done through four forms of settings file.
_Dependencies\Configuration\Listify-Settings.json.       - Baseline general settings for all environments.
_Dependencies\Configuration\Listify-Settings-XXXX.json   - Environment specific settings.
%PLISKYAPPROOT%\config\listify-settings-XXXX.donotcommit - Environment specific secrets.
%PLISKYAPPROOT%\config\listify-MACHINENAME-Override.json - Machine specific settings.

Environment variable set called PLISKYAPPROOT should have a folder underneath called "config", this can be anwhwere local on the machine.   
Secrets should be stored there in a file named listify-secrets.donotcommit.   Contact project authors for secrets.

