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

Bealzebub. 😈
Church. ⛪
Fairy.🧚
Ghost. 👻
JackOLantern. 🎃
Lizard. 🦎
Ogre. 👹



### Getting Started.



#### PreReqs

Visual Studio 2022, .net 9
Jira Board Access [here](https://plisky.atlassian.net/jira/software/projects/LFY)
nuke global tool [documentation here](https://nuke.build/docs/getting-started/installation)]

#### Steps.

Clone repository.
Open solution \sln\listify.sln.
Execute Solution ( VS 2022 Enterprise tested, Prerelease and Release)

Note that the solution sets listify.build to not compile in the default configuration, therefore to make changes to and compile the build engine you must right click on it and compile it directly.

### Build and CI.

Build using nuke.
Standard primary steps are Assemble, Construct, Examine, Package, Release.
Therefore to build and test the project run "nuke Examine" from the \src folder.

CI is currently NOT configured.


