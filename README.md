# Real-Time Locomotion on Soft Grounds with Dynamic Footprints

![teaser](Docs/Images/teaser.jpg)

- [Introduction](#Introduction)
- [Adaptive Model for Character Locomotion](#Model)
- [Citation](#Citation)
- [License](#License)


<a name="Introduction"></a>
## Introduction

This repository provides the codes used to reproduce the results shown in the following paper: **Real-Time Locomotion on Soft Grounds with Dynamic Footprints**. Eduardo Alvarado, Chloé Paliard, Damien Rohmer, Marie-Paule Cani.

<a name="Model"></a>
## Adaptive Model for Character Locomotion
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


<a name="Citation"></a>

## Citation

*In progress*

<a name="License"></a>

## License

The code is released under MIT License. See LICENSE for details.