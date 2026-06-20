<h2>COOL TOOLS</h2>
<h3> Version: 0.2.7</h3>

<br>
This is a collection of tools that aims to help speed up early prototyping and development of Unity projects.
It contains the following major Assemblies:
<ul>
<li>Actors (Actor engine for faster prototype development)</li>
<li>Attributes (Useful script attributes for the inspector)</li>
<li>Data (Data containers useful for game development)</li>
<li>Utilities (Utility functions and classes)</li>
</ul>

This readme focuses more on Actors.

### Actors
Actors (Actor Engine) was developed as a result of continuously using the same codebase during the development of small game prototypes.
Since creating the same systems over and over for new projects is obviously redundant and not desired, previous codebase was rolled over
to the next project, and so a coherent codebase for creating games in Unity was starting to take shape into what is known now as Actors.

### Design Philosophy
When using Actors assembly for your projects, most of the dynamic objects of your game might be Actors. An Actor is simply a component
that contains references to other components and some other small utility functions. It doesn't do much on it's own. But the Actor component
is the most important dependency for other components in the Actors assembly. It's most common use is establishing **Ownership**.

### Ownership
Components derived from the Actor assembly can have Owners (Actors). The OwnableBehaviour class is a class that derives from MonoBehaviour
and is the base class for all Ownable components (It implements IOwnable interface). So you can create and use components that have a specific 
Actor as it's owner.

It makes sense to have GameObjects in your game owned by an Actor, for example a projectile that your character shoots. This character object
contains an Actor component attached to it. So when it shoots a projectile, this projectile derives from OwnableBehaviour so now an owner
can be assigned to it. Now whenever this projectile hits something (or someone...) we now know who dealt the damage, and we can also
know everything about the Actor that shot that projectile, including it's stats (covered later) to calculate damage, for exmaple.

<br>
