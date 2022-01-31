# Real-Time Locomotion on Soft Grounds with Dynamic Footprints

![teaser](Docs/Images/teaser.jpg)

- [Introduction](#Introduction)
- [Real-time Terrain Deformation](#Terrain)
- [Instructions](#Instructions)
- [Results](#Results)
- [Citation](#Citation)
- [License](#License)


<a name="Introduction"></a>
## Introduction

This repository provides the codes used to reproduce the results shown in the following paper: **Real-Time Locomotion on Soft Grounds with Dynamic Footprints**. Eduardo Alvarado, Chloé Paliard, Damien Rohmer, Marie-Paule Cani.

Our system takes as input a character model, ie. a mesh geometry, a rigged skeleton with kinematic animations and an IK system applied to the feet bones, on top of a simple proxy-geometry and rigid body used for balance control and collision processing. Then, it builds a real-time locomotion model that combines a global controller to adjust the overall character pose based on the ground slope, enabling tilting and balancing behaviors, with an IK system to adapt the model to non-flat terrains.

In order to reproduce two-ways interactions between the model and the environment, the character deforms the soft ground based on a custom-made light-weight force model for feet-to-ground contact, driven by the kinematics on the input motion and the nature of the terrain. Finally, a versatile model based on Hooke's law is used for ground deformation, parametrized by the Young modulus in compression and by the Poisson ratio in lateral material displacement.

<p align="center">
  <img src="Docs/Gifs/knight-sand-walking.gif" width="40%">
&nbsp; &nbsp;
  <img src="Docs/Gifs/knight-sand-running.gif" width="40%">
</p>

<p align="center">
  <img src="Docs/Gifs/fairy-snow-running.gif" width="40%">
&nbsp; &nbsp;
  <img src="Docs/Gifs/fairy-snow-walking.gif" width="40%">
</p>
<p align="center"><em>Figure 1: Examples of footprints caused by different character morphologies in various ground types, such as snow or sand.</em></p>

<a name="Force"></a>
## Real-time Terrain Deformation

We propose a model for the forces that the character applies to the ground when its feet are in contact with it, based on its kinematics and the nature of the ground. The resulting interaction forces over time are used to compute a plausible ground deformation.

The static forces that the model exerts on the ground are estimated based on the character's mass *m*, the contact area between the feet and the ground and the balance described by the contribution of each foot to the character's weight. In addition, a dynamic force model during contact takes into consideration the force that each foot generates due to its change of momentum when it lands into the ground with certain velocity. In order to define the time needed for the character to be fully stopped by a given type of terrain, we introduce an external parameter called the *characteristic time τ*. Therefore, a given forward kinematics motion provided as input can be associated to different forces, ie. a large magnitude of momentum force on a hard terrain with small *τ* value, and small force magnitude with long effect on a soft terrain with large *τ*.

<p align="center">
  <img src="Docs/Gifs/walking-forces.gif" width="40%">
</p>
<p align="center"><em>Figure 2: Ground Reaction Forces generated during the kinematic animation.</em></p>

Finally, we use a linear plastic model for terrain compression along with a ray-casting method to map the estimated forces into the respective ground deformation. Parameters such as the Young Modulus of Elasticity *E* or Poisson ratio *ν* can be modified to change the behavior of the terrain under deformation.

<a name="Instructions"></a>
## Instructions

*In progress*

<a name="Results"></a>
## Results

<p align="center">
  <img src="Docs/Gifs/basic-footprints.gif" width="60%">
</p>

<p align="center">
  <img src="Docs/Images/terrains.jpg" width="100%">
</p>

<p align="center">
  <img src="Docs/Gifs/quad.gif" width="60%">
</p>

<a name="Citation"></a>
## Citation

*In progress*

<a name="License"></a>
## License

The code is released under MIT License. See LICENSE for details.