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
maxLimitX = 620
minLimitY = 400
maxLimitY = 1000

legendX = 0.0425

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


# Max and min GR Forces
GRForceLeftMax = max(GRForceLeft)
idxLeftMax = GRForceLeft.index(GRForceLeftMax)
GRForceRightMax = max(GRForceRight)
idxRightMax = GRForceRight.index(GRForceRightMax)

# Max and min momentum Forces
momentumLeftMax = max(momentumForceExertedByGroundLeft)
idxMomentumLeftMax = momentumForceExertedByGroundLeft.index(momentumLeftMax)
momentumRightMax = max(momentumForceExertedByGroundRight)
idxMomentumRightMax = momentumForceExertedByGroundRight.index(momentumRightMax)

# Create just a figure and only one subplot
fig, ax = plt.subplots(5)
fig.tight_layout()

###

# 1. Plot Weight Forces
ax[0].plot(idx, weightForceLeftY, label='Weight Force - Left Foot', color="midnightblue")
ax[0].plot(idx, weightForceRightY, label='Weight Force - Right Foot', color="royalblue")

ax[0].set_ylabel('Force (Y) [N]')
ax[0].set_xlabel('Timestamp')
ax[0].set_title('Weight Forces')

ax[0].legend(bbox_to_anchor=(0., 1.05, legendX, 0.), loc='lower left', ncol=1, mode="expand", borderaxespad=0.)

# Show max/min value with arrow
#ax[0].annotate(weightForceLeftY[idxLeftMax], xy=(idxLeftMax, weightForceLeftY[idxLeftMax]), xytext=(idxLeftMax, weightForceLeftY[idxLeftMax] + 315), arrowprops=dict(facecolor='black', shrink=0.01))
ax[0].annotate('{0:3.0f} N'.format(weightForceLeftY[idxMomentumLeftMax]), xy=(idxMomentumLeftMax, weightForceLeftY[idxMomentumLeftMax]), xytext=(idxMomentumLeftMax, weightForceLeftY[idxMomentumLeftMax] + 315), arrowprops=dict(facecolor='black', shrink=0.01))

ax[0].set_xlim([minLimitX, maxLimitX])
ax[0].set_ylim([-800, 50])
ax[0].grid()

###

# 2. Plot Momentum Forces
ax[1].plot(idx, momentumForceExertedByGroundLeft, label='Momentum Force - Left Foot', color="maroon")
ax[1].plot(idx, momentumForceExertedByGroundRight, label='Momentum Force - Right Foot', color="red")

ax[1].set_ylabel('Force (Y) [N]')
ax[1].set_xlabel('Timestamp')
ax[1].set_title('Positive Momentum Forces Exerted By Ground')

ax[1].legend(bbox_to_anchor=(0., 1.05, legendX, .102), loc='lower left', ncol=1, mode="expand", borderaxespad=0.)

#ax[1].annotate(momentumForceExertedByGroundLeft[idxLeftMax], xy=(idxLeftMax, momentumForceExertedByGroundLeft[idxLeftMax]), xytext=(idxLeftMax, momentumForceExertedByGroundLeft[idxLeftMax] + 150), arrowprops=dict(facecolor='black', shrink=0.01))
ax[1].annotate('{0:3.1f} N'.format(momentumForceExertedByGroundLeft[idxMomentumLeftMax]), xy=(idxMomentumLeftMax, momentumForceExertedByGroundLeft[idxMomentumLeftMax]), xytext=(idxMomentumLeftMax, momentumForceExertedByGroundLeft[idxMomentumLeftMax] + 150), arrowprops=dict(facecolor='green', shrink=0.01))

ax[1].set_xlim([minLimitX, maxLimitX])
ax[1].set_ylim([-50, 400])
ax[1].grid()

###

# 3. Plot GRFs per foot
ax[2].plot(idx, GRForceLeft, label='Ground Reaction Force - Left Foot', color="darkgreen")
ax[2].plot(idx, GRForceRight, label='Ground Reaction Force - Right Foot', color="lime")

#ax[2].annotate(GRForceLeftMax, xy=(idxLeftMax, GRForceLeftMax), xytext=(idxLeftMax, GRForceLeftMax + 400), arrowprops=dict(facecolor='green', shrink=0.01))
ax[2].annotate('{0:3.1f} N'.format(GRForceLeft[idxMomentumLeftMax]), xy=(idxMomentumLeftMax, GRForceLeft[idxMomentumLeftMax]), xytext=(idxMomentumLeftMax, GRForceLeft[idxMomentumLeftMax] + 400), arrowprops=dict(facecolor='black', shrink=0.01))

ax[2].set_ylabel('Force (Y) [N]')
ax[2].set_xlabel('Timestamp')
ax[2].set_title('Ground Reaction Force (GRF)')

ax[2].legend(bbox_to_anchor=(0., 1.05, legendX, .102), loc='lower left', ncol=1, mode="expand", borderaxespad=0.)

ax[2].set_xlim([minLimitX, maxLimitX])
ax[2].set_ylim([-100, 1000])
ax[2].grid()

###

# 4. Plot GRFs total
ax[3].scatter(idx, GRForce)
ax[3].plot(idx, np.abs(weightForceY), '-', label='Absolute Weight Force - Both feet', color="blue")

x_sm = np.array(idx)
y_sm = np.array(GRForce)
X_Y_Spline = make_interp_spline(x_sm, y_sm)
X_ = np.linspace(x_sm.min(), x_sm.max(), 1000)
Y_ = X_Y_Spline(X_)

ax[3].plot(X_, Y_, label='GRF - Both feet', color="limegreen")

ax[3].set_ylabel('Force (Y) [N]')
ax[3].set_xlabel('Timestamp')
ax[3].set_title('Ground Reaction Force (GRF)')

ax[3].set_xlim([minLimitX, maxLimitX])
ax[3].set_ylim([minLimitY, maxLimitY])

ax[3].annotate('{0:3.1f} N'.format(GRForce[idxMomentumLeftMax]), xy=(idxMomentumLeftMax, GRForce[idxMomentumLeftMax]), xytext=(idxMomentumLeftMax, GRForce[idxMomentumLeftMax] + 230), arrowprops=dict(facecolor='black', shrink=0.01))

ax[3].legend(bbox_to_anchor=(0., 1.05, legendX, .102), loc='lower left', ncol=1, mode="expand", borderaxespad=0.)

ax[3].grid()

###

# 5. Plot Normalized GRF
x_sm = np.array(idx)

GRForceNorm = [(x - 735.75) / 77.5 for x in GRForce]
y_sm = np.array(GRForceNorm)
X_Y_Spline = make_interp_spline(x_sm, y_sm)
X_ = np.linspace(x_sm.min(), x_sm.max(), 2000)
Y_ = X_Y_Spline(X_)

ax[4].plot(X_, Y_, label='Normalized GRF - Both feet', color="limegreen")

ax[4].set_ylabel('Force (Y) [N]')
ax[4].set_xlabel('Timestamp')
ax[4].set_title('Normalized Ground Reaction Force (GRF)')

ax[4].set_xlim([minLimitX, maxLimitX])
ax[4].set_ylim([-4, 4])

#ax[4].annotate('{0:.3g}'.format(GRForceNorm[idxLeftMax]), xy=(idxLeftMax, GRForceNorm[idxLeftMax]), xytext=(idxLeftMax, GRForceNorm[idxLeftMax] + 3), arrowprops=dict(facecolor='black', shrink=0.01))
ax[4].annotate('{0:.3g}'.format(GRForceNorm[idxMomentumLeftMax]), xy=(idxMomentumLeftMax, GRForceNorm[idxMomentumLeftMax]), xytext=(idxMomentumLeftMax, GRForceNorm[idxMomentumLeftMax] + 3), arrowprops=dict(facecolor='black', shrink=0.01))

ax[4].legend(bbox_to_anchor=(0., 1.05, legendX, .102), loc='lower left', ncol=1, mode="expand", borderaxespad=0.)

ax[4].grid()

###

plt.show()