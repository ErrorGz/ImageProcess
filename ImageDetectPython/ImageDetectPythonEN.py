
import time
from ultralytics import YOLO
import cv2
import numpy as np
import math
from collections import defaultdict


# Load the YOLOv8 model
model = YOLO('yolov8n-pose.pt')
# Create a color array
colors = [(255, 0, 0), (0, 255, 0), (0, 0, 255), (255, 255, 0), (0, 255, 255), (255, 0, 255), (128, 0, 0),
          (0, 128, 0), (0, 0, 128), (128, 128, 0), (0, 128, 128), (128, 0, 128), (128, 128, 128), (64, 0, 0),
          (0, 64, 0), (0, 0, 64), (64, 64, 0), (0, 64, 64), (64, 0, 64), (64, 64, 64)]


# Open the video file
video_path = "E:\BaiduSyncdisk\PY\DataSet\视频\9月14日.mp4"
RTSP='rtsp://guest:123456@10.103.8.236:554/avstream/channel=1/stream=1.sdp'
usb_cam=0

cap = cv2.VideoCapture(RTSP)

# Store the track history
track_history = defaultdict(lambda: [])

# Define variable to save current video time position, unit is milliseconds
current_time = 0

# Define the time interval for rewinding and fast forwarding, unit is milliseconds
skip_interval = 10000  # 10秒

# Define variable to calculate frame rate
fps_start_time = time.time()
fps_counter = 0

# Create a flag variable to control pause and resume
paused = False


# Loop through the video frames
while cap.isOpened():
    # Read a frame from the video
    success, frame = cap.read()

    if success:
			# Downscale the frame
            frame = cv2.resize(frame, (640, 480))

			# Run YOLOv8 tracking on the frame, persisting tracks between frames
            results = model.track(frame, persist=True)

			# Read the results
            boxes = results[0].boxes.xywh.cpu().tolist()
            cls_list = results[0].boxes.cls.int().cpu().tolist()
            track_ids = results[0].boxes.id.int().cpu().tolist() if results[0].boxes.id is not None else []
            keypoints = results[0].keypoints.xy.cpu()
            annotated_frame = results[0].orig_img

			# Plot the tracks
            for box, track_id in zip(boxes, track_ids):
                x, y, w, h = box 
                track = track_history[track_id]
                track.append((float(x), float(y)))  # x, y center point
                if len(track) > 30:  # retain 90 tracks for 90 frames
                     track.pop(0)

				# Draw the tracking lines
                points = np.hstack(track).astype(np.int32).reshape((-1, 1, 2))

				# Draw track_id text
                track_id_text = f"Track ID: {track_id}"
                cv2.putText(annotated_frame, track_id_text, (int(x-w/2), int(y-h/2)),
							cv2.FONT_HERSHEY_SIMPLEX, 0.75, (0, 0, 255), 2)

			

            for i in range(keypoints.shape[0]):
                points = keypoints[i].tolist()

				# Draw keypoint connections
                if len(points) == 17:
				
					# connecting middle points a and b 
                    p1 = points[1]
                    p2 = points[2] 
                    p3 = points[3]
                    p4 = points[4]
                    a = (int((p1[0] + p2[0]) / 2), int((p1[1] + p2[1]) / 2))
                    b = (int((p3[0] + p4[0]) / 2), int((p3[1] + p4[1]) / 2))

					# radius of circle
                    radius = 1.5 * max(math.sqrt((points[0][0] - p3[0]) ** 2 + (points[0][1] - p3[1]) ** 2),
									   math.sqrt((points[0][0] - p4[0]) ** 2 + (points[0][1] - p4[1]) ** 2))

					# coordinates of endpoint 
                    angle = math.atan2(a[1] - b[1], a[0] - b[0])
                    endpoint = ((int)(b[0] + radius * math.cos(angle)),
								(int)(b[1] + radius * math.sin(angle)))

					# Draw line segment
                    cv2.line(annotated_frame, b, endpoint,
							 colors[i % len(colors)], 2)

					# midpoint of point 15 and 16 as bottom 
                    bottomPointX = (points[15][0] + points[16][0]) / 2
                    bottomPointY = (points[15][1] + points[16][1]) / 2

					# Connect point 5 and 6
                    p5 = points[5]
                    p6 = points[6]
                    cv2.line(annotated_frame, (int(p5[0]), int(p5[1])),
							 (int(p6[0]), int(p6[1])),
							 colors[i % len(colors)], 2)

					# Connect point 11 and 12
                    p11 = points[11] 
                    p12 = points[12]
                    cv2.line(annotated_frame, (int(p11[0]), int(p11[1])),
							 (int(p12[0]), int(p12[1])),
							 colors[i % len(colors)], 2)

					# Connect point 5, 7 and 9
                    p7 = points[7]
                    p9 = points[9] 
                    cv2.line(annotated_frame, (int(p5[0]), int(p5[1])),
							 (int(p7[0]), int(p7[1])),
							 colors[i % len(colors)], 2)
                    cv2.line(annotated_frame, (int(p7[0]), int(p7[1])),
							 (int(p9[0]), int(p9[1])),
							 colors[i % len(colors)], 2)

                    p8 = points[8]
                    p10 = points[10]
                    cv2.line(annotated_frame, (int(p6[0]), int(p6[1])), 
							 (int(p8[0]), int(p8[1])),
							 colors[i % len(colors)], 2)
                    cv2.line(annotated_frame, (int(p8[0]), int(p8[1])),
							 (int(p10[0]), int(p10[1])),
							 colors[i % len(colors)], 2)

			# Calculate and display frame rate
            fps_counter += 1
            if time.time() - fps_start_time >= 1.0:
               fps = fps_counter / (time.time() - fps_start_time)
               fps_text = f"FPS: {round(fps, 2)}"                 
               fps_start_time = time.time()
               fps_counter = 0

            cv2.putText(annotated_frame, fps_text, (0, 30), cv2.FONT_HERSHEY_SIMPLEX, 1, (0, 255, 0), 2)
			# Display the annotated frame
            cv2.imshow("YOLOv8 Tracking", annotated_frame)

			# Update current time position
            current_time = cap.get(cv2.CAP_PROP_POS_MSEC)



			# Check if any key is pressed on the keyboard
            key = cv2.waitKey(1)

			# Pause/resume if space key is pressed  
            if key == ord(" "):
               paused = not paused  # Toggle pause state
               if paused:
                  print("Paused. Press any key to continue.")
               else:
                  print("Resumed.")

			# If in paused state, wait for any key press to continue
            if paused:
               cv2.waitKey(0)
               paused = False  # Toggle to non-paused state

			# Rewind 10 seconds if left key is pressed
            if key == ord("a"):
				# Calculate target time position 
               target_time = current_time - skip_interval

				# Adjust video time position
               cap.set(cv2.CAP_PROP_POS_MSEC, target_time)

				# Update current time position
               current_time = target_time  
               print("Rewind 10 seconds, current time position:", current_time)

			# Fast forward 10 seconds if right key is pressed  
            if key == ord("d"):
				# Calculate target time position
               target_time = current_time + skip_interval

				# Adjust video time position       
               cap.set(cv2.CAP_PROP_POS_MSEC, target_time)

				# Update current time position       
               current_time = target_time
               print("Fast forward 10 seconds, current time position:", current_time)



			# Break loop if q key or ESC (27) is pressed
            if key == ord("q") or key == 27:  
               break

    else:
		# Break the loop if the end of the video is reached
        break

# Release the video capture object and close the display window
cap.release()
cv2.destroyAllWindows()
