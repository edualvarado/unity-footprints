import numpy as np
import matplotlib.pyplot as plt

idx = []
gravityForceLeftY, gravityForceRightY = [],[]
speedFootLeftY, speedFootRightY = [],[]
impulseFootLeftY, impulseFootRightY = [],[]
netForceExertedByGroundLeft, netForceExertedByGroundRight = [],[]
totalForceLeft, totalForceRight = [],[]

minLimit = 80
maxLimit = 160

with open("..\..\plotting_cache\\total_forces.txt") as f:
    for index, line in enumerate(f):
        values = [float(s) for s in line.split(",")]
        idx.append(index)
        gravityForceLeftY.append(values[0])
        gravityForceRightY.append(values[1])
        speedFootLeftY.append(values[2])
        speedFootRightY.append(values[3])
        impulseFootLeftY.append(values[4])
        impulseFootRightY.append(values[5])
        netForceExertedByGroundLeft.append(values[6])
        netForceExertedByGroundRight.append(values[7])
        totalForceLeft.append(values[8])
        totalForceRight.append(values[9])

# Max and min total Forces
totalForceLeftMax = max(totalForceLeft)
totalForceRightMax = max(totalForceRight)
idxLeftMax = totalForceLeft.index(totalForceLeftMax)
idxRightMax = totalForceRight.index(totalForceRightMax)

# Create just a figure and only one subplot
fig, ax = plt.subplots(5)
fig.tight_layout()

# 1. Plot Gravity Forces
ax[0].plot(idx, gravityForceLeftY, label='Gravity Force - Left Foot', color="midnightblue")
ax[0].plot(idx, gravityForceRightY, label='Gravity Force - Right Foot', color="royalblue")

ax[0].set_ylabel('Gravity Force [N]')
ax[0].set_xlabel('Timestamp (s)')
ax[0].set_title('Gravity Force')
ax[0].legend(loc = "lower left")

ax[0].set_xlim([minLimit, maxLimit])
ax[0].grid()

###

# 2. Plot Speeds
ax[1].plot(idx, speedFootLeftY, label='Speed Y - Left Foot', color="darkorange")
ax[1].plot(idx, speedFootRightY, label='Speed Y - Right Foot', color="gold")

ax[1].set_ylabel('Speed Y [m/s^2]')
ax[1].set_xlabel('Timestamp (s)')
ax[1].set_title('Feet Speed')
ax[1].legend(loc = "lower left")

ax[1].set_xlim([minLimit, maxLimit])
ax[1].grid()

###

# 3. Plot Impulses
ax[2].plot(idx, impulseFootLeftY, label='Impulse Y - Left Foot', color="teal")
ax[2].plot(idx, impulseFootRightY, label='Impulse Y - Right Foot', color="cyan")

ax[2].set_ylabel('Impulse Y [Ns]')
ax[2].set_xlabel('Timestamp (s)')
ax[2].set_title('Impulse')
ax[2].legend(loc = "lower left")

ax[2].set_xlim([minLimit, maxLimit])
ax[2].grid()

###

# 4. Plot Net Forces
ax[3].plot(idx, netForceExertedByGroundLeft, label='Net Force Exerted by Ground Y - Left Foot', color="maroon")
ax[3].plot(idx, netForceExertedByGroundRight, label='Net Force Exerted by Ground Y - Right Foot', color="red")

ax[3].set_ylabel('Net Force Y [N]')
ax[3].set_xlabel('Timestamp (s)')
ax[3].set_title('Net Force')
ax[3].legend(loc = "lower left")

ax[3].set_xlim([minLimit, maxLimit])
ax[3].grid()

###

# 5. Plot Total Forces
ax[4].plot(idx, totalForceLeft, label='Total Ground Reaction Force Y - Left Foot', color="darkgreen")
ax[4].plot(idx, totalForceRight, label='Total Ground Reaction Force Y - Right Foot', color="lime")

ax[4].annotate(totalForceLeftMax, xy=(idxLeftMax, totalForceLeftMax), xytext=(idxLeftMax, totalForceLeftMax+185), arrowprops=dict(facecolor='black', shrink=0.01))

ax[4].set_ylabel('Ground Reaction Force Y [N]')
ax[4].set_xlabel('Timestamp (s)')
ax[4].set_title('Ground Reaction Force')
ax[4].legend(loc = "lower left")

ax[4].set_xlim([minLimit, maxLimit])
ax[4].grid()

plt.show()