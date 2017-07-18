# CXA Cleaner UI

This code is strictly developed and used for the Hackthon only. 

## Design

### Architecture

#### Server—Client

- Server only relays the instruction between the clients (end users) and agents (robots) in the final design. In the first design, the server will do all the calculation and planning parts.
- Client (end users) will generate the grids from [image](./CXACleanerUI/res/floorG1.png) like a blue print, with labels/flags on each grid, indicating it as blocked/dirty/planned to clean etc. The labels are System.Int32 and operated using bit operation (check MappingConstants in [constants.cs](./CXACleanerUI/constants.cs))
- Agent (robot cleaners) will only operate according to the commands from server. However, it should still have some basic mechanism like obstacle avoidance, auto-docking for recharge

#### Communication

##### Encoding

> Designed by Zhiwen

- A char x.
- \n — End of the command
- 'A' -> — Go forward for x - 'A' steps
- 'a' -> — Go backward for x - 'a' steps
- '(' — steer left
- ')' — steer right

### Algorithm

#### Grid Generation

- Pixel translation based on RGB value threshold.
- The status (blocked or be able to pass through) of a greater block is based on One-vote Veto: If there is any pixel blocks the grid, the whole grid is treated as blocked.

#### Routing

- Snake Shape Filling to minimum the times of steering. (Inappropriate expression?)
- A* to navigate the agent from one block of selected area to another.
- Steering to change the facing direction: Assuming the vehicle can only go backwards in a straight line.

#### Flags

- Block/Unblock — Highest priority, absolutely control the behavior
- Select/Unselect — Overwrite Clean/Dirty, sometimes plan/unplan
- Plan/Unplan — Specific flag

### Communication

- Sockets
- Python server is running on another laptop and the code is not included.

## Some thoughts

Well, it is the first time I participate in Hackthon with totally new pre-experience in C# XD but I still chose it to have coherence with my teammates though they can code in various languages, some overlaps with my skillsets also and finally we still have a Python server and Arduino Edison robot (Arduino C). There are some fun parts, but mostly challenges.

### Management

Though the question statement was released several weeks before the contest, we did not really put effort into developing but rather just read through it, chose one and only brainstormed some rough idea. This made the development part heavy and uncontrollable. We also 'lost our focus' as after first presentation we believed that the main focus should be on the solution, the idea, instead of the actually working prototype. Thus a Unity project may even be better — agile development, easy testing, avoiding annoying hardware inconsistency issues.

Plan ahead, with details. Try to trade more time before the contest. It is better to play during the 24h rather than death march for the deadline.

### Cooperation

In a contest of teamwork, high level of cooperation is needed to excel. It is really helpful to split the developing tasks into parts (Me — Algorithm [Map generation, routing, backend part of agent status tracking and management]; Tianzhen — UI, Network and Communication, the Middleware between me and Zhiwen; Zhiwen — hardware, all of the robots) during death march. However, it is a bad idea to ignoring what your teammates are doing. Every member may have different workload and work in a different pace. While the parts of the solution are highly interrelated, no in-time response can be received if you are working when your teammates are "offline" (snapping). This will be worth if you don't know what they are really doing, or how to run their code. Near the deadline, one may need to continue on other's job as it may have higher priority and harder to be implemented.

### Technical

#### Version control

Essential

- Use branches, merge and rebase in git.
- Merge frequently.
  - Merge to master only after completely implement a function such that the Main() function will be affected.
  - Pull then merge. Immediately push after merge.
  - Check every line while resolving conflicts.
  - Tell your team that you have pushed a new version
- Run tests before commit, or at least before push. 
- Stage every file when temporary changes are made, before testing. IDE may accidentally revert the modification if you work in another editor. (Atom and Visual Studio for me)
- Commit files in the order that all the version can be successfully compiled. Try to commit only one file each time.
- Commit Renaming/code beautifying separately to avoid unnecessary rollbacks. (For example, roll a function code back but have to revert the code beautifying as well)
- Write the commit log concise and clear. May leave some blur commits if the changes are obvious and not important (in some rare cases)
- Push all the works before going "offline"

There are still much more, but I can only come out with these at this time.

<u>Important</u>: Design data structure and functionalities first, then interface, then develop. Stick on "documents" (which may not really exist at last due to the time constrain).

#### Code Style

- Write comments when necessary.
  - At the beginning of one function/a chunk of code
  - Main functionality
  - Side-effects
  - Additional requirements
  - Time and space complexity if needed
  - TODOs if needed
  - Exceptions if possible
- Naming
  - Follow correspond code guide
  - Full, meaningful identifier
- Exception handleing
  - Must be implemented in UI programming

#### Coding

- Don't eat too much
- Rest several times in between
- Be prepared

## [Question Statement](https://portal.imda.gov.sg/~/media/ITSC/Files/CXA%202017%20Challenge%20Statements.pdf)

Quoted from the official documents. Code: CXA-07

**<u>Challenge Title</u>**: Georectification on robotic cleaner

<u>**Description of the problem:**</u>

Mall cleaners have extensive interior and exterior walkways with stone/tile flooring to clean. There is a need to deploy a fleet of robot cleaners to augment mall cleaners. A cleaner is required to guide the robot cleaning machine around the area to be cleaned as part of the mapping process. Following this mapping process, the robot cleaner is then deployed to clean the mapped area. Should there be any obstruction along the mapped route, the robot would stop its operations and wait for human intervention.

**<u>Challenges:</u>**

To propose and develop a solution that addressed one or more of the following challenges:

- To automate the mapping of areas that robot cleaners are supposed to clean. The current mapping process is too manual, time consuming and not a very smart way to execute.
- To incorporate an intelligent logic such that the robot cleaner can circumvent any obstruction along the mapped route.
- To automate the recharging of robot cleaners: detect low battery and navigate to charging bay. Currently, there is a need to manually deploy a cleaner to bring the machine to the charging bay.
- To propose a solution where robot cleaners (perhaps in a fleet of 2 or 3 machines) working as a team with the cleaner so that the cleaner could cover the areas that the machine cannot be deployed to clean.
- To propose a solution using sensors and wireless IoT to gain greater visibility in fleet management of the robot cleaners.

There should be analytics that would

- validate the areas cleaned
- generate data on the areas cleaned
- analyse the deployment of the cleaning team and scheduling etc

## Credits

Thanks guys for all the works, patience and much more!

As a team member, I really enjoy the time with you all.

Hope to work with you one day again o(≧v≦)o

As time passes, we may be far away from each other. But this repo, inscription of our 24 hours, will always remind us of the brilliant time we have ever experienced together.

------

Team members: Hongyu Teng, Tianzhen Ni, Zhiwen Yan

Team No Code No Life

National Junior College