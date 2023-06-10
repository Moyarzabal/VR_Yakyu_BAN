# VR_Yakyu_BAN
<img width="744" alt="スクリーンショット 2023-06-10 13 42 42" src="https://github.com/Moyarzabal/VR_Yakyu_BAN/assets/92244620/ce4da131-7b46-4aae-a625-d497535b31e8">

This is a baseball game for two players.
Two controllers of Oculus Quest 2 are held by a pitcher player and a hitter player. 
The pitcher's controller determines the pitch speed (proportional to the actual swing speed of the controller) and the direction of the breaking ball (specified by the joystick), and the hitter's controller swings the bat.


**Hitting** 
I implemented a bat that follows the Oculus controller and set the force vector applied to the ball when the bat hits the ball to make it closer to reality.

**Pitching**
I had a particularly difficult time implementing breaking balls. 
I wanted to implement parabolas at all angles, up, down, left, and right, so I used a combination of moving the ball on a trajectory complemented by a second-order Bézier curve and applying force to the ball to make it move. 

**Other Feartures**
In addition, I made it possible to switch to a viewpoint that follows the ball as it is being hit, reflected the hit results on the runner status and scoreboard, and added sound effects and BGM to increase the sense of realism. 
