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
#totalForceMax = max(totalForce)
#idxMax = totalForce.index(totalForceMax)

###

# 1. Plot Total Forces
plt.plot(idx, totalForce, label='Total Ground Reaction Force (Y) - GRF', color="green")
plt.scatter(idx, totalForce)

plt.plot(idx, gravityForceY, '-', label='Total Gravity Force (Y) (Abs)', color="blue")

#plt.annotate(totalForceMax, xy=(idxMax, totalForceMax), xytext=(idxMax, totalForceMax+185), arrowprops=dict(facecolor='black', shrink=0.01))

#

x_sm = np.array(idx)
y_sm = np.array(totalForce)

X_Y_Spline = make_interp_spline(x_sm, y_sm)

X_ = np.linspace(x_sm.min(), x_sm.max(), 1000)
Y_ = X_Y_Spline(X_)

plt.plot(X_, Y_, color="red")

#

plt.ylabel('Force (Y) [N]')
plt.xlabel('Timestamp [s]')
plt.title('Ground Reaction Force - Walking Gait')
plt.legend(loc = "lower left")

plt.xlim([minLimitX, maxLimitX])
plt.ylim([minLimitY, maxLimitY])

plt.grid()

###

plt.show()