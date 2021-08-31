import matplotlib.pyplot as plt
import time
import numpy as np
from scipy.interpolate import make_interp_spline, BSpline

idx = []
gravityForceLeftY, gravityForceRightY, gravityForceY = [],[],[]
netForceExertedByGroundLeft, netForceExertedByGroundRight, netForceExertedByGround = [],[],[]
totalForceLeft, totalForceRight, totalForce = [],[],[]

minLimitX = 475
maxLimitX = 600
minLimitY = 400
maxLimitY = 1000

with open("..\..\plotting_cache\OLD\Finalv4\\all_forces_walking_gait.txt") as f:
    for index, line in enumerate(f):
        values = [float(s) for s in line.split(",")]
        idx.append(index)
        gravityForceLeftY.append(values[0])
        gravityForceRightY.append(values[1])
        gravityForceY.append(values[2])
        netForceExertedByGroundLeft.append(values[3])
        netForceExertedByGroundRight.append(values[4])
        netForceExertedByGround.append(values[5])
        totalForceLeft.append(values[6])
        totalForceRight.append(values[7])
        totalForce.append(values[8])


# Max and min total Forces
totalForceLeftMax = max(totalForceLeft)
idxLeftMax = totalForceLeft.index(totalForceLeftMax)

# Create just a figure and only one subplot
fig, ax = plt.subplots(3)
fig.tight_layout()

###

# 1. Plot Gravity Forces
ax[0].plot(idx, gravityForceLeftY, label='Gravity Force - Left Foot', color="midnightblue")
ax[0].plot(idx, gravityForceRightY, label='Gravity Force - Right Foot', color="royalblue")

ax[0].set_ylabel('Force (Y) [N]')
ax[0].set_xlabel('Timestamp [s]')
ax[0].set_title('Gravity Forces - Walking Gait')
ax[0].legend(loc = "lower left")

ax[0].annotate(gravityForceLeftY[idxLeftMax], xy=(idxLeftMax, gravityForceLeftY[idxLeftMax]), xytext=(idxLeftMax, gravityForceLeftY[idxLeftMax]+185), arrowprops=dict(facecolor='black', shrink=0.01))

ax[0].set_xlim([minLimitX, maxLimitX])
ax[0].grid()

###

# 2. Plot Net Forces
ax[1].plot(idx, netForceExertedByGroundLeft, label='Net Force Exerted by Ground (Y) - Left Foot', color="maroon")
ax[1].plot(idx, netForceExertedByGroundRight, label='Net Force Exerted by Ground (Y) - Right Foot', color="red")

ax[1].set_ylabel('Net Force (Y) [N]')
ax[1].set_xlabel('Timestamp [s]')
ax[1].set_title('Positive Net Forces Exerted By Ground - Walking Gait')
ax[1].legend(loc = "lower left")

ax[1].set_xlim([minLimitX, maxLimitX])
ax[1].grid()

###

# 3. Plot Total Forces
ax[2].plot(idx, totalForceLeft, label='Ground Reaction Force (Y) - Left Foot', color="darkgreen")
ax[2].plot(idx, totalForceRight, label='Ground Reaction Force (Y) - Right Foot', color="lime")

ax[2].annotate(totalForceLeftMax, xy=(idxLeftMax, totalForceLeftMax), xytext=(idxLeftMax, totalForceLeftMax+185), arrowprops=dict(facecolor='black', shrink=0.01))

ax[2].set_ylabel('Force (Y) [N]')
ax[2].set_xlabel('Timestamp [s]')
ax[2].set_title('Ground Reaction Forces (GRF) - Walking Gait')
ax[2].legend(loc = "lower left")

ax[2].set_xlim([minLimitX, maxLimitX])
ax[2].grid()

###

plt.show()