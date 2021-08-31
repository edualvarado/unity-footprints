import matplotlib.pyplot as plt
import time
import numpy as np
from scipy.interpolate import make_interp_spline, BSpline

# Values
idx = []
weightForceLeftY, weightForceRightY, weightForceY = [], [], []
momentumForceExertedByGroundLeft, momentumForceExertedByGroundRight, momentumForceExertedByGround = [], [], []
GRForceLeft, GRForceRight, GRForce = [], [], []

# Limits plot
minLimitX = 475
maxLimitX = 600
minLimitY = 400
maxLimitY = 1000

# Data file and value assignation
with open("..\..\plotting_cache\\all_forces_walking_gait.txt") as f:
    for index, line in enumerate(f):
        values = [float(s) for s in line.split(",")]
        idx.append(index)
        weightForceLeftY.append(values[0])
        weightForceRightY.append(values[1])
        weightForceY.append(values[2])
        momentumForceExertedByGroundLeft.append(values[3])
        momentumForceExertedByGroundRight.append(values[4])
        momentumForceExertedByGround.append(values[5])
        GRForceLeft.append(values[6])
        GRForceRight.append(values[7])
        GRForce.append(values[8])


# Max and min total Forces
GRForceLeftMax = max(GRForceLeft)
idxLeftMax = GRForceLeft.index(GRForceLeftMax)
GRForceRightMax = max(GRForceRight)
idxRightMax = GRForceRight.index(GRForceRightMax)

# Create just a figure and only one subplot
fig, ax = plt.subplots(5)
fig.tight_layout()

###

# 1. Plot Weight Forces
ax[0].plot(idx, weightForceLeftY, label='Weight Force - Left Foot', color="midnightblue")
ax[0].plot(idx, weightForceRightY, label='Weight Force - Right Foot', color="royalblue")

ax[0].set_ylabel('Force (Y) [N]')
ax[0].set_xlabel('Timestamp')
ax[0].set_title('Weight Forces - Walking Gait')
ax[0].legend(loc = "lower left")

# Show max/min value with arrow
ax[0].annotate(weightForceLeftY[idxLeftMax], xy=(idxLeftMax, weightForceLeftY[idxLeftMax]), xytext=(idxLeftMax, weightForceLeftY[idxLeftMax] + 185), arrowprops=dict(facecolor='black', shrink=0.01))

ax[0].set_xlim([minLimitX, maxLimitX])
ax[0].grid()

###

# 2. Plot Momentum Forces
ax[1].plot(idx, momentumForceExertedByGroundLeft, label='Momentum Force Exerted by Ground - Left Foot', color="maroon")
ax[1].plot(idx, momentumForceExertedByGroundRight, label='Momentum Force Exerted by Ground - Right Foot', color="red")

ax[1].set_ylabel('Momentum Force (Y) [N]')
ax[1].set_xlabel('Timestamp')
ax[1].set_title('Positive Momentum Forces Exerted By Ground - Walking Gait')
ax[1].legend(loc = "lower left")

ax[1].set_xlim([minLimitX, maxLimitX])
ax[1].grid()

###

# 3. Plot GRFs per foot
ax[2].plot(idx, GRForceLeft, label='Ground Reaction Force - Left Foot', color="darkgreen")
ax[2].plot(idx, GRForceRight, label='Ground Reaction Force - Right Foot', color="lime")

ax[2].annotate(GRForceLeftMax, xy=(idxLeftMax, GRForceLeftMax), xytext=(idxLeftMax, GRForceLeftMax + 185), arrowprops=dict(facecolor='black', shrink=0.01))

ax[2].set_ylabel('Force (Y) [N]')
ax[2].set_xlabel('Timestamp')
ax[2].set_title('Ground Reaction Force (GRF) - Walking Gait')
ax[2].legend(loc = "lower left")

ax[2].set_xlim([minLimitX, maxLimitX])
ax[2].grid()

###

# 4. Plot GRFs total
ax[3].scatter(idx, GRForce)
ax[3].plot(idx, np.abs(weightForceY), '-', label='Absolute Weight Force - Total', color="blue")

x_sm = np.array(idx)
y_sm = np.array(GRForce)
X_Y_Spline = make_interp_spline(x_sm, y_sm)
X_ = np.linspace(x_sm.min(), x_sm.max(), 1000)
Y_ = X_Y_Spline(X_)

ax[3].plot(X_, Y_, label='Ground Reaction Force - Total', color="lime")

ax[3].set_ylabel('Force (Y) [N]')
ax[3].set_xlabel('Timestamp')
ax[3].set_title('Ground Reaction Force (GRF) - Walking Gait')

ax[3].set_xlim([minLimitX, maxLimitX])
ax[3].set_ylim([minLimitY, maxLimitY])

ax[3].legend(loc = "lower left")

ax[3].grid()

###

# 5. Plot Normalized GRF
x_sm = np.array(idx)

GRForceNorm = [(x - 735.75) / 77.5 for x in GRForce]
y_sm = np.array(GRForceNorm)
X_Y_Spline = make_interp_spline(x_sm, y_sm)
X_ = np.linspace(x_sm.min(), x_sm.max(), 2000)
Y_ = X_Y_Spline(X_)

ax[4].plot(X_, Y_, label='Normalized Ground Reaction Force - Total', color="lime")

ax[4].set_ylabel('Force (Y) [N]')
ax[4].set_xlabel('Timestamp')
ax[4].set_title('Normalized Ground Reaction Force (GRF) - Walking Gait')

ax[4].set_xlim([minLimitX, maxLimitX])
ax[4].set_ylim([-3, 3])

ax[4].legend(loc = "lower left")

ax[4].grid()

###

plt.show()