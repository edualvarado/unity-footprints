---
layout: default
title: "Real-Time Locomotion on Soft Grounds with Dynamic Footprints"
---

<!-- <center><h1>{{ page.title }}</h1></center> -->

<td>
	<center>
		Frontiers in VR - 2022<br>
		<br>
		<nobr>Eduardo Alvarado (1)</nobr> &emsp;&emsp; <nobr>Chloé Paliard (2)</nobr> &emsp;&emsp; <nobr>Damien Rohmer (1)</nobr> &emsp;&emsp; <nobr>Marie-Paule Cani (2)</nobr><br>
		<br>
		<nobr>(1) LIX, Ecole Polytechnique/CNRS, Institut Polytechnique de Paris, Palaiseau, France</nobr> &emsp;&emsp; <nobr>(2) Télécom Paris, Institut Polytechnique
de Paris, Palaiseau, France</nobr><br>
		<br>
		<img style="vertical-align:middle" src="Images/teaser.jpg"  width="100%" height="inherit"/>		
	</center>
</td>

<br>
	
<td>
	<hr>
	<h3 style="margin-bottom:10px;">Abstract</h3>
  When we move on snow, sand, or mud, the ground deforms under our feet, immediately
  affecting our gait. We propose a physically based model for computing such interactions in
  real time, from only the kinematic motion of a virtual character. The force applied by each
  foot on the ground during contact is estimated from the weight of the character, its current
  balance, the foot speed at the time of contact, and the nature of the ground. We rely on a
  standard stress-strain relationship to compute the dynamic deformation of the soil under
  this force, where the amount of compression and lateral displacement of material are,
  respectively, parameterized by the soil’s Young modulus and Poisson ratio. The resulting
  footprint is efficiently applied to the terrain through procedural deformations of refined
  terrain patches, while the addition of a simple controller on top of a kinematic character
  enables capturing the effect of ground deformation on the character’s gait. As our results
  show, the resulting footprints greatly improve visual realism, while ground compression
  results in consistent changes in the character’s motion. Readily applicable to any
  locomotion gait and soft soil material, our real-time model is ideal for enhancing the
  visual realism of outdoor scenes in video games and virtual reality applications.
</td>

<td>
	<h3> Paper: [<a href="https://www.frontiersin.org/articles/10.3389/frvir.2022.801856/full">PDF</a>] &nbsp; &nbsp; &nbsp; Code: [<a href="https://github.com/edualvarado/unity-footprints">GitHub</a>] &nbsp; &nbsp; &nbsp; Media: [<a href="">BAIR</a> / <a href="">Tech Review</a>] &nbsp; &nbsp; &nbsp; Preprint: [<a href="">arXiv</a>] </h3>
</td>

<tr>
		<h3 style="margin-bottom:10px;">Videos</h3>
		<iframe width="560" height="315" src="" frameborder="0" allow="autoplay; encrypted-media" allowfullscreen></iframe>
		<br><br>
		<iframe width="560" height="315" src="" frameborder="0" allow="autoplay; encrypted-media" allowfullscreen></iframe>
</tr>
	
<br>
<br>

<h3 style="margin-bottom:0px;">Bibtex</h3>
<pre>
@ARTICLE{10.3389/frvir.2022.801856,
AUTHOR={Alvarado, Eduardo and Paliard , Chloé and Rohmer , Damien and Cani , Marie-Paule},    
TITLE={Real-Time Locomotion on Soft Grounds With Dynamic Footprints},      
JOURNAL={Frontiers in Virtual Reality},      
VOLUME={3},      
YEAR={2022},      
URL={https://www.frontiersin.org/article/10.3389/frvir.2022.801856},       
DOI={10.3389/frvir.2022.801856},      
ISSN={2673-4192},   
ABSTRACT={When we move on snow, sand, or mud, the ground deforms under our feet, immediately affecting our gait. We propose a physically based model for computing such interactions in real time, from only the kinematic motion of a virtual character. The force applied by each foot on the ground during contact is estimated from the weight of the character, its current balance, the foot speed at the time of contact, and the nature of the ground. We rely on a standard stress-strain relationship to compute the dynamic deformation of the soil under this force, where the amount of compression and lateral displacement of material are, respectively, parameterized by the soil’s Young modulus and Poisson ratio. The resulting footprint is efficiently applied to the terrain through procedural deformations of refined terrain patches, while the addition of a simple controller on top of a kinematic character enables capturing the effect of ground deformation on the character’s gait. As our results show, the resulting footprints greatly improve visual realism, while ground compression results in consistent changes in the character’s motion. Readily applicable to any locomotion gait and soft soil material, our real-time model is ideal for enhancing the visual realism of outdoor scenes in video games and virtual reality applications.}
}
</pre>
