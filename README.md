# Black Hole Simulation

An interactive 3D black hole visualization built with C# and MonoGame. The project combines a simplified gravitational particle simulation with a procedurally generated accretion disk, animated particles, and an orbiting camera.

This is a graphics and physics programming prototype, not a scientifically accurate general-relativity simulation. The gravity model is Newtonian-inspired, and the lensing-like visual effects are created by reshaping geometry rather than tracing light through curved spacetime.

# Features

- Gravitational particle motion: 200 particles are initialized in a 3D volume and accelerated toward the black hole using an inverse-square gravity model. Particles are respawned when their lifetime expires or they reach the center.
- Procedural 3D geometry: The black hole and accretion disk are constructed from generated vertices and triangle indices rather than prebuilt 3D models.
- Animated accretion disk: A rotating disk mesh uses a warm-white-to-dark-red color gradient and 250 independently animated particles with randomized orbital radii, speeds, and heights.
- Lensing-inspired effects: Warped copies of the disk geometry create stylized funnel and dome shapes above and below the disk.
- Interactive camera: Orbit around the scene, change your viewing angle, and zoom in or out with mouse or keyboard controls.
Controls
Input	Action
Right mouse button + drag	Orbit the camera
Mouse wheel	Zoom in / out
A / D	Rotate camera horizontally
Q / E	Adjust camera pitch
W / S	Zoom in / out
Esc	Exit

# How it works

The main Simulation class manages the MonoGame update and draw loops. Each frame, the simulation updates elapsed-time-based animations and particle positions, then renders the 3D scene using view and projection matrices.

The gravitational acceleration is based on an inverse-square relationship:
\[
\mathbf{a} = \frac{M}{\max(r^2,\,0.1)}\,\hat{\mathbf{r}}
\]
Here, M is a configurable simulated mass, r is the particle's distance from the black hole, and the direction vector points toward the black hole. The denominator is clamped to reduce extreme acceleration near the center. This is a deliberately simplified model without physical units or relativistic effects.

The accretion disk is generated from concentric rings, divided into segments and connected by triangles. Vertex colors vary with radius. Additional copies of the geometry are distorted into dome- and funnel-like forms for visual effect. Disk particles follow animated circular paths separate from the gravitationally accelerated particles in the surrounding scene.
Source code overview
File	Responsibility
Program.cs	Starts the MonoGame application
Simulation.cs	Game loop, input, camera controls, and scene rendering
BlackHole.cs	Gravitational acceleration applied to particles
Particle.cs	Particle position, velocity, lifetime, and respawning
AccretionDisk.cs	Disk mesh generation, animation, color gradients, and visual overlays
DiskParticles.cs	Individual accretion-disk particle motion
BlackHoleRender.cs	Additional black hole sphere rendering

# Running the project

1. Clone or download the repository.
2. Open its solution or project file in Visual Studio, with the compatible .NET SDK installed.
3. Restore the project dependencies, including MonoGame, and build the project.
4. Run the application and use the controls above to explore the scene.
If the repository includes a runnable .csproj project, you can also use dotnet restore followed by dotnet run from that project's directory.
Scope and possible improvements
The simulation prioritizes learning 3D graphics, vector mathematics, procedural geometry, and interactive animation. It does not calculate general-relativistic trajectories, physically accurate accretion dynamics, or optical gravitational lensing.

Potential next steps include consolidating the rendering and camera code, reducing per-frame graphics allocations, and adding automated tests for the gravity calculations.