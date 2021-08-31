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
        gravityForceY.append(-values[2])
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
fig, ax = plt.subplots(4)
fig.tight_layout()

###

# 1. Plot Gravity Forces per Feet
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

# 2. Plot Total Forces per Feet
ax[1].plot(idx, totalForceLeft, label='Total Ground Reaction Force (Y) - Left Foot', color="darkgreen")
ax[1].plot(idx, totalForceRight, label='Total Ground Reaction Force (Y) - Right Foot', color="lime")

ax[1].annotate(totalForceLeftMax, xy=(idxLeftMax, totalForceLeftMax), xytext=(idxLeftMax, totalForceLeftMax+185), arrowprops=dict(facecolor='black', shrink=0.01))

ax[1].set_ylabel('Force (Y) [N]')
ax[1].set_xlabel('Timestamp [s]')
ax[1].set_title('Ground Reaction Forces - Walking Gait')
ax[1].legend(loc = "lower left")

ax[1].set_xlim([minLimitX, maxLimitX])
ax[1].grid()

###

# 3. Plot Total GRF
#ax[2].plot(idx, totalForce, label='Ground Reaction Force (Y) - Total', color="green")
ax[2].scatter(idx, totalForce)
ax[2].plot(idx, gravityForceY, '-', label='Total Gravity Force (Y) (Abs)', color="blue")

x_sm = np.array(idx)
y_sm = np.array(totalForce)
X_Y_Spline = make_interp_spline(x_sm, y_sm)
X_ = np.linspace(x_sm.min(), x_sm.max(), 1000)
Y_ = X_Y_Spline(X_)

ax[2].plot(X_, Y_, label='Total Ground Reaction Force (Y) - GRF', color="lime")

ax[2].set_ylabel('Force (Y) [N]')
ax[2].set_xlabel('Timestamp [s]')
ax[2].set_title('Ground Reaction Forces (GRF) - Walking Gait')

ax[2].set_xlim([minLimitX, maxLimitX])
ax[2].set_ylim([minLimitY, maxLimitY])

ax[2].legend(loc = "lower left")

ax[2].grid()

###

# 4. Plot Total GRF Normalized by body weight
x_sm = np.array(idx)

totalForceNorm = [(x - 735.75) / 77.5 for x in totalForce]
y_sm = np.array(totalForceNorm)
X_Y_Spline = make_interp_spline(x_sm, y_sm)
X_ = np.linspace(x_sm.min(), x_sm.max(), 2000)
Y_ = X_Y_Spline(X_)

ax[3].plot(X_, Y_, label='Normalized Total Ground Reaction Force (Y) - GRF', color="lime")

ax[3].set_ylabel('Force (Y) [N]')
ax[3].set_xlabel('Timestamp [s]')
ax[3].set_title('Normalized Ground Reaction Forces (GRF) - Walking Gait')

ax[3].set_xlim([minLimitX, maxLimitX])
ax[3].set_ylim([-3, 3])

ax[3].legend(loc = "lower left")

ax[3].grid()

###

plt.show()
