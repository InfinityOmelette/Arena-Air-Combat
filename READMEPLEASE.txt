============================
          CONTROLS
============================

-------- Mouse control -------

Flight:
 - Green diamond indicates desired autopilot direction
 - Move mouse to move green diamond, airplane will automatically roll, pitch, and yaw towards desired direction
 - You can manually adjust pitch, roll, and yaw by using W and S for pitch, D and A for roll, and Q and E for yaw. Once the axis key is released, the autopilot resumes control of that axis.
 - Scroll mousewheel to change throttle
 - Hold SHIFT for free look. Allows camera  movement without changing autopilot direction

Combat:
 - Left click to shoot machine guns
 - Right click to launch secondary weapon

Weapon Selection:
 - numbers 1-4 (NOT on numpad) to select weapon
 - Fwd/Back button on mouse to step to next or previous weapon
 - TAB key also steps to next weapon

Targeting Computer:
 - Click middle mouse button to select enemy target closest to center of screen. Selected target
  will have their icon blink.
 - Minimap will automatically scale to show target
 - To lock on, you must be using a weapon with appropriate radar, be within range, and have target within scan cone. Target icon will turn red, and you'll hear a continuous high pitch lock tone
   > Maverick: air-to-ground lock only. Cannot lock air targets
   > AMRAAM: air-to-air missile only. Cannot lock ground targets

Missile Evasion:
 - Press F to drop flares. For 1 second, this will make you invisible to any radar, be it SAM, aircraft, or weapon radar. While performing evasive maneuvers, pop these flares 1 second before missile strikes you.
   > these are a limited resource. you have 4 flare slots, each reload individually. Reload time is lengthy, so use the flares sparingly

Landing gear: 
 - press G to toggle landing gear
 - When landing gear is down, more drag is applied to aircraft. You can use this as an airbrake

Map:
 - Press M to step map scale in and out.
 - Range number shows the distance from top of map, to your location.


------------  XBOX 360 CONTROLLER  ----------------

Move left stick for pitch and roll
Triggers for rudder left and right
Bumpers for throttle up and down
D-pad to select weapon
Move right stick to move camera
X to toggle landing gear
Y to step map scale in and out
B to fire secondary weapon
A to fire machine gun
Click left stick to drop flares
Click right stick to select target


-------- Camera/Input Toggle Controls -----------

"O" to toggle mouse autopilot mode
"L" to toggle camera-leveling. (When deactivated, camera rolls with aircraft, instead of staying level to horizon)
"P" to toggle controller camera aim
  - when inactive, lower right will show text boxes for weapon names, instead of graphical images
  - mouse autopilot mode overrides this, because mouse moves camera anyways
  - active, right stick on controller will move camera. (mouse autopilot must be off)


================================
     HUD SYMBOLOGY
================================

Square: Aircraft
Triangle: AAA (anti-air gun vehicle)
Half-circle: SAM (Surface-to-air missile vehicle)
Hexagon: Ground unit (Tanks, artillery, rocket trucks)


=============================================
       Missile Flight Mechanics, Evasion
=============================================

Missiles use the same aerodynamics script that player does.

Missiles have limited onboard fuel.

So, the slower the missile goes, the less maneuverable it is.

To evade a missile, get the missile to slow down enough that you are more maneuverable than it.

Evasive maneuvers in any direction will make the missile perform course 
corrections, bleeding its speed.

When missile is a few seconds away, it's usually good to make sure you're turning in ANY other direction than directly towards, or directly away from the missile. Imagine a line extending forward infinitely through the missile's line of travel. You need to get as far away from this line as possible.

However, if you are too close to the missile at the time of launch, you simply will not have the time to evade enough to bleed the missile's speed enough. Only a well-timed flare will save you in this case. 

Respect the range of enemy missile launchers, and keep your distance until you have an opportunity to safely close

====================================
     RADAR AND DETECTION MECHANICS
====================================
This game uses very simple "radar" mechanics.

You will detect an enemy if they are:
 1. Within your scan zone, AND
 2. There is unbroken line of sight, AND
 3. Within detection range

SAM's have a spherical scan zone. You just have to be within range, and unbroken line of sight

Fighters (you) have a conical scan zone in front
 - 60 degree cone in front of you
 - Detection range: ~6km

Your player can still visually detect enemies within 1.5km. Beyond that, you need to use your radar



====================================
    RADAR WARNING RECEIVER (RWR)
====================================

Your aircraft can detect when an enemy or missile has you within its radar scan zone

Indicators placed along a ring-shaped zone around central HUD show the bearing of enemy radars that have you in their radar scan zone with an unbroken line of sight

Ex: You see a SAM indicator above the central HUD:
  - enemy SAM is in the direction the camera is pointed
  - There is an unbroken line of sight between you and this SAM
  - This does NOT necessarily mean the radar detects you. You are in its scan zone, but you may need to be closer for it to actually see you. (IRL, radar energy goes out basically forever, but the radar reflection only goes a relatively short distance)

The number of "v" marks indicate how close the threat is. The more marks, the closer the threat

Ex: this symbol at the above of the central HUD.
  SAM
   v
   v
   v

 this indicates a VERY close SAM threat, in front of you

"FTR" is an enemy Fighter contact.

--------------  Missile Warnings  -----------

RWR indicators in the center of the screen are red, and are enemy missiles that are tracking you
 - The indicator that is flashing yellow and red is the closest enemy missile tracking you
 - The indicator flashes yellow faster and faster as the missile gets closer -- the "INCOMING MISSILE" red label flashes on and off faster as well

"MSL" is a surface-to-air missile launched by a SAM vehicle. Relatively shorter ranged missile

"AMR" is an AMRAAM air-to-air missile launched by an enemy Fighter


===================================
      DATALINK
===================================

This is literally just spotting enemies so your teammates can see them, even if they don't have line of sight

This happens automatically. Any enemy that you detect, your teammates can see them as well

If you see a "DL" next to an enemy icon, this means a friendly unit also detects that enemy

SAM's can contribute to datalink, as well as friendly Fighter Jets















