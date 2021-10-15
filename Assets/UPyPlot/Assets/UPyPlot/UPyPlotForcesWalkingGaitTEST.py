import matplotlib.pyplot as plt
import time
import numpy as np
from scipy.interpolate import make_interp_spline, BSpline

# Values
idx = []
weightForceLeftY, weightForceRightY, weightForceY = [], [], []

# Limits plot
minLimitX = 0
maxLimitX = 4
minLimitY = -1000
maxLimitY = 0

legendX = 0.0425

# Data file and value assignation
with open("..\..\plotting_cache\\testtest.txt") as f:
    for index, line in enumerate(f):
        values = [float(s) for s in line.split(",")]
        idx.append(index * 0.1)
        weightForceLeftY.append(values[0])
        weightForceRightY.append(values[1])

# Create just a figure and only one subplot
fig, ax = plt.subplots(2)
fig.tight_layout()

###

# 1. Plot Weight Forces
ax[0].plot(idx, weightForceLeftY, label='Weight Force - Left Foot', color="midnightblue")
ax[0].plot(idx, weightForceRightY, label='Weight Force - Right Foot', color="royalblue")

ax[0].set_ylabel('Force (Y) [N]')
ax[0].set_xlabel('Timestamp')
ax[0].set_title('Weight Forces')

ax[0].legend(bbox_to_anchor=(0., 1.05, legendX, 0.), loc='lower left', ncol=1, mode="expand", borderaxespad=0.)

ax[0].set_xlim([minLimitX, maxLimitX])
ax[0].set_ylim([-800, 50])
ax[0].grid()

###

plt.show()