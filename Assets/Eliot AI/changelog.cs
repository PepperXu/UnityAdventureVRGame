/*
 * Version 1.1
 * |----1. Integration with A* Pathfinding Project Pro
 * |----2. Eliot Motion system's implementation is now detached from Motion class itself and works through
 *         IMotionEngine interface, letting the User to extend Eliot Motion by adding new custom pathfinding engines.
 * |----3. Eliot's Standard Library is extended
 *      |----3.1. Inventory Condition Interface is extended
 *      |----3.2. Inventory Action Interface is extended
 *      |----3.3. Perception Condition Interface is extended
 * |----4. Changes to classes names
 *      |----4.1. Behaviour is now EliotBehaviour
 *      |----4.2. Component is now EliotComponent
 *      |----4.3. Animation is now AgentAnimation
 * |----5. Warnings about unused variables in Eliot's Condition and Action Interfaces are now suppressed
 * |----6. IntegrationManager added. Configures Unity's preprocessor #define directives to automatically know what
 *         parts of code to compile depending on the installed packages.
 * |----7. Unit Factory's Motion section redesigned so that the User can specify the desired Motion Engine
 * |----8. Added properties to Skill class that allow to check if a Skill adds/reduces health/energy
 *
 * Version 1.2
 * |----1. Control over time. Schedule actions using Behaviours.
 *      |----1.1. Schedule actions execution.
 *      |----1.2. Check at runtime whether it is time to do certain action or not.
 * |----2. Upgraded behaviours serialization.
 *      |----2.1. Behaviour components are now serialized by method names, making it more consistent
 *                 across multiple users.
 *      |----2.2. Backwards compatibility is provided.
 * |----3. Improved way of extending the library of actions and conditions.
 *      |----3.1. Now User can create custom classes to group actions better.
 *      |----3.2. Each group is encapsulated in file making it easier to share the code along with the Behaviours.
 * |----4. Animator Rootmotion support.
 * |----5. PlayMaker integration.
 *      |----5.1. Make noise that Agents can hear and react to.
 *      |----5.2. Let any object cast Skills just like Eliot Agents can.
 *      |----5.3. Interact with an agent by raycasting and applying a given Skill to a target agent.
 * |----6. Individual sounds are now replaced with arrays of possible sounds from which
 *         a random one is chosen every time.
 * |----7. An environment variable "ELIOT" is now added to the project letting the User check whether Eliot
 *         is currently added  to the project using preprocessor directive.
 *
 * Version 1.2.1
 * |----1. Integration with Invector Character Controllers.
 *      |----1.1. Invector Character Controllers can damage Eliot Agents.
 *      |----1.2. Eliot Agents can damage Invector Character Controllers.
 * |----2. Integration with Opsive UCC.
 *      |----2.1. Opsive UCC characters can damage Eliot Agents.
 *      |----2.2. Eliot Agents can damage Opsive UCC characters.
 * |----3. Integration with Two Cubes GKC.
 *      |----3.1. Two Cubes GKC characters can damage Eliot Agents.
 *      |----3.2. Eliot Agents can damage Two Cubes GKC characters.
 * |----4. Integration with UFPS.
 *      |----4.1. UFPS characters can damage Eliot Agents.
 *      |----4.2. Eliot Agents can damage UFPS characters.
 * |----5. Integration with Ootii TPMC.
 *      |----5.1. Ootii TPMC characters can damage Eliot Agents.
 *      |----5.2. Eliot Agents can damage Ootii TPMC characters.
 * |----6. Basic integration with Pixelcrushers Love/Have.
 * |----7. Minor bux fixes.
 */