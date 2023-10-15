
import time
from ultralytics import YOLO
import cv2
import numpy as np
import math
from collections import defaultdict


# Load the YOLOv8 model
model = YOLO('yolov8n-pose.pt')
# 新建颜色数组
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

# 定义变量来保存当前视频的时间位置，单位为毫秒
current_time = 0

# 定义回退和前进的时间间隔，单位为毫秒
skip_interval = 10000  # 10秒

# 定义变量来计算帧率
fps_start_time = time.time()
fps_counter = 0

# 创建一个标志变量，用于控制暂停和继续
paused = False


# Loop through the video frames
while cap.isOpened():
    # Read a frame from the video
    success, frame = cap.read()

    if success:
        # 对frame缩小比例
        frame = cv2.resize(frame, (640, 480))

        # Run YOLOv8 tracking on the frame, persisting tracks between frames
        results = model.track(frame, persist=True)

        # 读取结果
        boxes = results[0].boxes.xywh.cpu().tolist()
        cls_list = results[0].boxes.cls.int().cpu().tolist()
        track_ids = results[0].boxes.id.int().cpu().tolist() if results[0].boxes.id is not None else []
        keypoints = results[0].keypoints.xy.cpu()
        annotated_frame = results[0].orig_img

        # for i, (track_id, cls, box, points) in enumerate(zip(track_ids, cls_list, boxes, keypoints)):
        #     # 绘制追行跟进
        #     cv2.rectangle(annotated_frame, (int(box[1]), int(box[0])), (int(box[3]), int(box[2])), colors[cls], 2)
        #     cv2.putText(annotated_frame, str(track_id), (int(box[0]), int(box[1])), cv2.FONT_HERSHEY_SIMPLEX, 0.5, colors[cls], 2)

        # Plot the tracks
        for box, track_id in zip(boxes, track_ids):
            x, y, w, h = box
            track = track_history[track_id]
            track.append((float(x), float(y)))  # x, y center point
            if len(track) > 30:  # retain 90 tracks for 90 frames
                track.pop(0)

            # Draw the tracking lines
            points = np.hstack(track).astype(np.int32).reshape((-1, 1, 2))
            # cv2.polylines(annotated_frame, [points], isClosed=False, color=(
            #     230, 230, 230), thickness=10)

            # 绘制track_id
            track_id_text = f"Track ID: {track_id}"
            cv2.putText(annotated_frame, track_id_text, (int(x-w/2), int(y-h/2)),
                        cv2.FONT_HERSHEY_SIMPLEX, 0.75, (0, 0, 255), 2)



        for i in range(keypoints.shape[0]):
            points = keypoints[i].tolist()

            box = boxes[i] if boxes is not None and len(boxes) > i else None
            cls = cls_list[i] if cls_list is not None and len(cls_list) > i else None
            name = results[0].names[cls] if cls_list is not None and len(cls_list) > i and cls_list[i] is not None else "Default Name"

            # center_x, center_y, width, height = box if box is not None else (None, None, None, None)
            # # 绘制 box
            # if center_x is not None and center_y is not None and width is not None and height is not None:
            #     top_left = (int(center_x - width / 2), int(center_y - height / 2))
            #     bottom_right = (int(center_x + width / 2), int(center_y + height / 2))
            #     cv2.rectangle(annotated_frame, top_left, bottom_right, colors[cls], 2)
            #     cv2.putText(annotated_frame, str(name), top_left, cv2.FONT_HERSHEY_SIMPLEX, 0.5, colors[cls], 2)


            # 绘制关键点连线
            if len(points) == 17:
                # 计算并连接中间点a和b
                p1 = points[1]
                p2 = points[2]
                p3 = points[3]
                p4 = points[4]
                a = (int((p1[0] + p2[0]) / 2), int((p1[1] + p2[1]) / 2))
                b = (int((p3[0] + p4[0]) / 2), int((p3[1] + p4[1]) / 2))

                # 计算圆的半径
                radius = 1.5 * max(math.sqrt((points[0][0] - p3[0]) ** 2 + (points[0][1] - p3[1]) ** 2),
                                   math.sqrt((points[0][0] - p4[0]) ** 2 + (points[0][1] - p4[1]) ** 2))

                # 计算线段的终点坐标
                angle = math.atan2(a[1] - b[1], a[0] - b[0])
                endpoint = ((int)(b[0] + radius * math.cos(angle)),
                            (int)(b[1] + radius * math.sin(angle)))

                # 绘制线段
                cv2.line(annotated_frame, b, endpoint,
                         colors[i % len(colors)], 2)

                # 计算第15点和第16点的中间点作为底
                bottomPointX = (points[15][0] + points[16][0]) / 2
                bottomPointY = (points[15][1] + points[16][1]) / 2

                # 连接第5点和第6点
                p5 = points[5]
                p6 = points[6]
                cv2.line(annotated_frame, (int(p5[0]), int(p5[1])),
                         (int(p6[0]), int(p6[1])),
                         colors[i % len(colors)], 2)

                # 连接第11点和第12点
                p11 = points[11]
                p12 = points[12]
                cv2.line(annotated_frame, (int(p11[0]), int(p11[1])),
                         (int(p12[0]), int(p12[1])),
                         colors[i % len(colors)], 2)

                # 连接第5点、第7点和第9点
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

        # 计算并显示帧率
        fps_counter += 1
        if time.time() - fps_start_time >= 1.0:
            fps = fps_counter / (time.time() - fps_start_time)
            print("FPS:", round(fps, 2))
            fps_text = f"FPS: {round(fps, 2)}"                 
            fps_start_time = time.time()
            fps_counter = 0

        cv2.putText(annotated_frame, fps_text, (0, 30), cv2.FONT_HERSHEY_SIMPLEX, 1, (0, 255, 0), 2)
        # Display the annotated frame
        cv2.imshow("YOLOv8 Tracking", annotated_frame)

        # 更新当前时间位置
        current_time = cap.get(cv2.CAP_PROP_POS_MSEC)



        # 检查是否按下了键盘上的键
        key = cv2.waitKey(1)

        # 按下空格键暂停/继续
        if key == ord(" "):
            paused = not paused  # 切换暂停状态
            if paused:
                print("Paused. Press any key to continue.")
            else:
                print("Resumed.")

        # 如果处于暂停状态，等待按下任意键继续
        if paused:
            cv2.waitKey(0)
            paused = False  # 切换为非暂停状态

        # 按下左键回退10秒
        if key == ord("a"):
            # 计算目标时间位置
            target_time = current_time - skip_interval

            # 调整视频的时间位置
            cap.set(cv2.CAP_PROP_POS_MSEC, target_time)

            # 更新当前时间位置
            current_time = target_time
            print("回退10秒，当前时间位置:", current_time)

        # 按下右键前进10秒
        if key == ord("d"):
            # 计算目标时间位置
            target_time = current_time + skip_interval

            # 调整视频的时间位置
            cap.set(cv2.CAP_PROP_POS_MSEC, target_time)

            # 更新当前时间位置
            current_time = target_time
            print("前进10秒，当前时间位置:", current_time)



        # 按下q退出循环
        if key == ord("q") or key == 27:  # 27 对应 ESC 键的 ASCII 值
            break

    else:
        # Break the loop if the end of the video is reached
        break

# Release the video capture object and close the display window
cap.release()
cv2.destroyAllWindows()
