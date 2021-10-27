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
#minLimitX = 475
#minLimitX = 480
#maxLimitX = 620

#minLimitX = 150
#maxLimitX = 350

# For time in the axis x
minLimitX = 0
maxLimitX = 15

minLimitY = 400
maxLimitY = 1000

legendX = 0.0425

# To convert axis x to ms
time_interval_data = 0.1
offset = 480

# Data file and value assignation
with open("..\..\plotting_cache\\all_forces_walking_gait.txt") as f:
    for index, line in enumerate(f):
        values = [float(s) for s in line.split(",")]
        idx.append((index - offset) * (time_interval_data))
        weightForceLeftY.append(values[0])
        weightForceRightY.append(values[1])
        weightForceY.append(values[2])
        momentumForceExertedByGroundLeft.append(-values[3])
        momentumForceExertedByGroundRight.append(-values[4])
        momentumForceExertedByGround.append(-values[5])
        GRForceLeft.append(values[6])
        GRForceRight.append(values[7])
        GRForce.append(values[8])


# Max and min GR Forces
GRForceLeftMax = max(GRForceLeft)
idxLeftMax = GRForceLeft.index(GRForceLeftMax)
GRForceRightMax = max(GRForceRight)
idxRightMax = GRForceRight.index(GRForceRightMax)

# Max and min momentum Forces
momentumLeftMax = min(momentumForceExertedByGroundLeft)
idxMomentumLeftMax = momentumForceExertedByGroundLeft.index(momentumLeftMax)
idxMomentumLeftMaxSecond = (momentumForceExertedByGroundLeft.index(momentumLeftMax) - offset) * (time_interval_data)
momentumRightMax = min(momentumForceExertedByGroundRight)
idxMomentumRightMax = momentumForceExertedByGroundRight.index(momentumRightMax)

print("idxMomentumLeftMax ", idxMomentumLeftMax)
print("idxMomentumLeftMaxSecond ", idxMomentumLeftMaxSecond)
print("weightForceLeftY[idxMomentumLeftMax] ", weightForceLeftY[idxMomentumLeftMax])

# Create just a figure and only one subplot
fig, ax = plt.subplots(5)
fig.tight_layout()

###

# 1. Plot Weight Forces
ax[0].plot(idx, weightForceLeftY, label='Left Foot', color="midnightblue")
ax[0].plot(idx, weightForceRightY, label='Right Foot', color="royalblue")

ax[0].set_ylabel('Force (Y) [N]')
ax[0].set_xlabel('Time [s]')
ax[0].set_title('Weight Forces')

ax[0].legend(bbox_to_anchor=(0., 1.05, legendX, 0.), loc='lower left', ncol=1, mode="expand", borderaxespad=0.)

# Show max/min value with arrow
#ax[0].annotate(weightForceLeftY[idxLeftMax], xy=(idxLeftMax, weightForceLeftY[idxLeftMax]), xytext=(idxLeftMax, weightForceLeftY[idxLeftMax] + 315), arrowprops=dict(facecolor='black', shrink=0.01)) #+315
ax[0].annotate('{0:3.0f} N'.format(weightForceLeftY[idxMomentumLeftMax]), xy=(idxMomentumLeftMaxSecond, weightForceLeftY[idxMomentumLeftMax]), xytext=(idxMomentumLeftMaxSecond, weightForceLeftY[idxMomentumLeftMax] + 315), arrowprops=dict(facecolor='black', shrink=0.01))

ax[0].set_xlim([minLimitX, maxLimitX])
ax[0].set_ylim([-800, 50])
ax[0].grid()

###

# 2. Plot Momentum Forces
ax[1].plot(idx, momentumForceExertedByGroundLeft, label='Left Foot', color="maroon")
ax[1].plot(idx, momentumForceExertedByGroundRight, label='Right Foot', color="red")

ax[1].set_ylabel('Force (Y) [N]', labelpad=3)
ax[1].set_xlabel('Time [s]')
#ax[1].set_title('Positive Momentum Forces Exerted By Ground')
ax[1].set_title('Momentum Forces')

ax[1].legend(bbox_to_anchor=(0., 1.05, legendX, .102), loc='lower left', ncol=1, mode="expand", borderaxespad=0.)

#ax[1].annotate(momentumForceExertedByGroundLeft[idxLeftMax], xy=(idxLeftMax, momentumForceExertedByGroundLeft[idxLeftMax]), xytext=(idxLeftMax, momentumForceExertedByGroundLeft[idxLeftMax] + 150), arrowprops=dict(facecolor='black', shrink=0.01))
ax[1].annotate('{0:3.1f} N'.format(momentumForceExertedByGroundLeft[idxMomentumLeftMax]), xy=(idxMomentumLeftMaxSecond, momentumForceExertedByGroundLeft[idxMomentumLeftMax]), xytext=(idxMomentumLeftMaxSecond + 0.35, momentumForceExertedByGroundLeft[idxMomentumLeftMax] + 15), arrowprops=dict(facecolor='green', shrink=0.01))

ax[1].set_xlim([minLimitX, maxLimitX])
ax[1].set_ylim([-400, 50])
ax[1].grid()

###

# 3. Plot GRFs per foot
ax[2].plot(idx, GRForceLeft, label='Left Foot', color="darkgreen")
ax[2].plot(idx, GRForceRight, label='Right Foot', color="lime")

#ax[2].annotate(GRForceLeftMax, xy=(idxLeftMax, GRForceLeftMax), xytext=(idxLeftMax, GRForceLeftMax + 400), arrowprops=dict(facecolor='green', shrink=0.01)) # + 400
ax[2].annotate('{0:3.1f} N'.format(GRForceLeft[idxMomentumLeftMax]), xy=(idxMomentumLeftMaxSecond, GRForceLeft[idxMomentumLeftMax]), xytext=(idxMomentumLeftMaxSecond, GRForceLeft[idxMomentumLeftMax] + 400), arrowprops=dict(facecolor='black', shrink=0.01))

ax[2].set_ylabel('Force (Y) [N]')
ax[2].set_xlabel('Time [s]')
ax[2].set_title('Ground Reaction Forces')

ax[2].legend(bbox_to_anchor=(0., 1.05, legendX, .102), loc='lower left', ncol=1, mode="expand", borderaxespad=0.)

ax[2].set_xlim([minLimitX, maxLimitX])
ax[2].set_ylim([-100, 1000])
ax[2].grid()

###

# 4. Plot GRFs total
ax[3].scatter(idx, GRForce)
ax[3].plot(idx, np.abs(weightForceY), '-', label='|Weight Force|', color="blue")

x_sm = np.array(idx)
y_sm = np.array(GRForce)
X_Y_Spline = make_interp_spline(x_sm, y_sm)
X_ = np.linspace(x_sm.min(), x_sm.max(), 1000)
Y_ = X_Y_Spline(X_)

ax[3].plot(X_, Y_, label='GRF', color="limegreen")

ax[3].set_ylabel('Force (Y) [N]')
ax[3].set_xlabel('Time [s]')
#ax[3].set_title('Ground Reaction Force (GRF)')
ax[3].set_title('Total contribution on both feet')

ax[3].set_xlim([minLimitX, maxLimitX])
ax[3].set_ylim([minLimitY, maxLimitY])

ax[3].annotate('{0:3.1f} N'.format(GRForce[idxMomentumLeftMax]), xy=(idxMomentumLeftMaxSecond, GRForce[idxMomentumLeftMax]), xytext=(idxMomentumLeftMaxSecond, GRForce[idxMomentumLeftMax] + 230), arrowprops=dict(facecolor='black', shrink=0.01))

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

ax[4].set_ylabel('Normalized Force (Y) [N/kg]')
ax[4].set_xlabel('Time [s]')
#ax[4].set_title('Normalized Ground Reaction Force (GRF)')
ax[4].set_title('Normalized Ground Reaction Force')

ax[4].set_xlim([minLimitX, maxLimitX])
ax[4].set_ylim([-4, 4])

#ax[4].annotate('{0:.3g}'.format(GRForceNorm[idxLeftMax]), xy=(idxLeftMax, GRForceNorm[idxLeftMax]), xytext=(idxLeftMax, GRForceNorm[idxLeftMax] + 3), arrowprops=dict(facecolor='black', shrink=0.01))
ax[4].annotate('{0:.3g}'.format(GRForceNorm[idxMomentumLeftMax]), xy=(idxMomentumLeftMaxSecond, GRForceNorm[idxMomentumLeftMax]), xytext=(idxMomentumLeftMaxSecond, GRForceNorm[idxMomentumLeftMax] + 3), arrowprops=dict(facecolor='black', shrink=0.01))

#ax[4].legend(bbox_to_anchor=(0., 1.05, legendX, .102), loc='lower left', ncol=1, mode="expand", borderaxespad=0.)

ax[4].grid()

###

plt.show()