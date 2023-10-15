from ultralytics import YOLO
import time
import cv2
import numpy as np
import math
import threading
from collections import defaultdict, deque
import os
import matplotlib.font_manager as fm
from PIL import ImageFont, ImageDraw, Image


class ThreadSafeDataset:
    def __init__(self, max_frames=100):
        self.dataset = deque(maxlen=max_frames)
        self.lock = threading.Lock()

    def add_frame(self, frame, processed_frame, detection_result):
        # with self.lock:
            if len(self.dataset) >= self.dataset.maxlen:
                self.dataset.popleft()  # 删除最早的图像

            frame_data = {
                'frame': frame,
                'processed_frame': processed_frame,
                'detection_result': detection_result
            }

            self.dataset.append(frame_data)

    def get_frames(self):
        # with self.lock:
            return list(self.dataset)


def check_file_availability(file_name, extension):
    for i in range(1000):
        file_name_with_index = f"{file_name}{str(i).zfill(3)}.{extension}"
        if not os.path.exists(file_name_with_index):
            return True, file_name_with_index
    return False, None

def video_reader_thread(video_path, dataset):
    global ExitFlag , paused , advance, reverse,record,recording, current_time, skip_interval,fps


    video_writer=None

    while not ExitFlag:
        cap = cv2.VideoCapture(video_path)
        fps = cap.get(cv2.CAP_PROP_FPS)
    
        start_time = time.time()   # 记录循环开始时间

        while cap.isOpened() and not ExitFlag:
            start_time = time.time()   # 更新循环开始时间
            #如果paused为true则继续while循环
            if paused:

                continue
    
            ret, frame = cap.read()
            if not ret:
                break
            # 缩小尺寸
            #frame = cv2.resize(frame, (640,480))
            
            dataset.add_frame(frame, None, None)

            if advance:
                # 前进10秒操作
                advance = False
            
                # 计算目标时间位置
                target_time = current_time + skip_interval

                # 调整视频的时间位置
                cap.set(cv2.CAP_PROP_POS_MSEC, target_time)

                # 更新当前时间位置
                current_time = target_time

                print("前进10秒，当前时间位置:", int(current_time))


            if reverse:  
                # 后退10秒操作
                reverse = False
                
                # 计算目标时间位置
                target_time = current_time - skip_interval

                # 调整视频的时间位置
                cap.set(cv2.CAP_PROP_POS_MSEC, target_time)

                # 更新当前时间位置
                current_time = target_time
                print("回退10秒，当前时间位置:",int( current_time)    )

            if record:
                if video_writer is None:
                    check,  output_file = check_file_availability("CapVideo","mp4")
                    if check ==True:
                        fourcc = cv2.VideoWriter_fourcc(*"avc1")  # 使用H.264编解码器
                        frame_size = (640, 480)  # 帧尺寸为640x480像素
                        video_writer = cv2.VideoWriter(output_file, fourcc, fps, frame_size)
                        recording=True
                video_writer.write(frame)
                
            else:
                if video_writer is not None:
                    video_writer.release()
                    video_writer = None
                    recording=False
    
            current_time = cap.get(cv2.CAP_PROP_POS_MSEC)
        

            end_time = time.time()  # 记录循环结束时间
            elapsed_time = end_time - start_time  # 计算循环执行时间

            # 计算动态延时
            frame_delay = 1 / fps - elapsed_time
            if frame_delay > 0:
                time.sleep(frame_delay)
            
            if frame_delay < 0:
                skip_frames = int(abs(frame_delay) * fps) + 1
                for _ in range(skip_frames):
                    cap.read()

        cap.release()
        print("视频结束")
        ExitFlag=True



def yolo_detection_thread(dataset):
    global ExitFlag ,paused # 声明 ExitFlag 是全局变量

    print("初始化YOLOv8模型")
    # 初始化 YOLOv8 模型
    model = YOLO('yolov8n-pose.pt')
    track_history = defaultdict(list)
    print("初始化完毕")
    paused=False

    while not ExitFlag:
        frames = dataset.get_frames()

        if len(frames) > 0:
            latest_frame_data = frames[-1]
            frame = latest_frame_data['frame']

            results = model.track(frame, persist=True,verbose=False)
            # processed_frame=results[0].plot()
            processed_frame = draw_annotations(frame, results, track_history)
  

            # 将检测结果保存到帧的数据集中
            latest_frame_data['processed_frame']=processed_frame
            latest_frame_data['detection_result'] = results

        time.sleep(0.1)  # 这里可以根据需要调整检测的时间间隔


    
def draw_annotations(frame, results, track_history):
    colors = [(0, 255, 0), (0, 0, 255), (255, 0, 0)]  # 定义不同类别的颜色

    boxes = results[0].boxes.xywh.cpu().tolist()
    cls_list = results[0].boxes.cls.int().cpu().tolist()
    track_ids = results[0].boxes.id.int().cpu().tolist() if results[0].boxes.id is not None else []
    keypoints = results[0].keypoints.xy.cpu()
    annotated_frame = frame.copy()

    # Plot the tracks
    for track_id,cls,box,keypoint in zip(track_ids,cls_list,boxes,keypoints):
        name=results[0].names[cls]
        x, y, w, h = box
        # track = track_history[track_id]
        # track.append((float(x), float(y)))  # x, y center point
        # if len(track) > 30:  # retain 90 tracks for 90 frames
        #     track.pop(0)

        # # Draw the tracking lines
        # points = np.hstack(track).astype(np.int32).reshape((-1, 1, 2))
        # cv2.polylines(annotated_frame, [points], isClosed=False, color=(
        #     230, 230, 230), thickness=1)

        #绘制外框
        x1, y1, x2, y2 = x - w / 2, y - h / 2, x + w / 2, y + h / 2
        cv2.rectangle(annotated_frame, (int(x1), int(y1)),
                        (int(x2), int(y2)), colors[track_id % len(colors)], 2)
        
        # 绘制track_id
        track_id_text = f"Track ID: {track_id}"
        cv2.putText(annotated_frame, track_id_text, (int(x-w/2), int(y-h/2-15)),
                    cv2.FONT_HERSHEY_SIMPLEX, 0.5, (255, 255, 255),1)
        
        #绘制LabelName
        cv2.putText(annotated_frame, name, (int(x1), int(y1 -5)),
                    cv2.FONT_HERSHEY_SIMPLEX, 0.5, (255, 255, 255), 1)        

        # 绘制关键点连线
        if len(keypoint) == 17:
            # 计算并连接中间点a和b
            p1 = keypoint[1]
            p2 = keypoint[2]
            p3 = keypoint[3]
            p4 = keypoint[4]
            a = (int((p1[0] + p2[0]) / 2), int((p1[1] + p2[1]) / 2))
            b = (int((p3[0] + p4[0]) / 2), int((p3[1] + p4[1]) / 2))

            # 计算圆的半径
            radius = 1.5 * max(math.sqrt((keypoint[0][0] - p3[0]) ** 2 + (keypoint[0][1] - p3[1]) ** 2),
                               math.sqrt((keypoint[0][0] - p4[0]) ** 2 + (keypoint[0][1] - p4[1]) ** 2))

            # 计算线段的终点坐标
            angle = math.atan2(a[1] - b[1], a[0] - b[0])
            endpoint = ((int)(b[0] + radius * math.cos(angle)),
                        (int)(b[1] + radius * math.sin(angle)))

            # 绘制线段
            cv2.line(annotated_frame, b, endpoint,
                     colors[track_id % len(colors)], 2)

            # 计算第15点和第16点的中间点作为底
            bottomPointX = (keypoint[15][0] + keypoint[16][0]) / 2
            bottomPointY = (keypoint[15][1] + keypoint[16][1]) / 2

            # 连接第5点和第6点
            p5 = keypoint[5]
            p6 = keypoint[6]
            cv2.line(annotated_frame, (int(p5[0]), int(p5[1])),
                     (int(p6[0]), int(p6[1])),
                     colors[track_id % len(colors)], 2)

            # 连接第11点和第12点
            p11 = keypoint[11]
            p12 = keypoint[12]
            cv2.line(annotated_frame, (int(p11[0]), int(p11[1])),
                    (int(p12[0]), int(p12[1])),
                     colors[track_id % len(colors)], 2)

            # 连接第5点、第7点和第9点
            p7 = keypoint[7]
            p9 = keypoint[9]
            cv2.line(annotated_frame, (int(p5[0]), int(p5[1])),
                        (int(p7[0]), int(p7[1])),
                        colors[track_id % len(colors)], 2)
            cv2.line(annotated_frame, (int(p7[0]), int(p7[1])),
                        (int(p9[0]), int(p9[1])),
                        colors[track_id % len(colors)], 2)

            p8 = keypoint[8]
            p10 = keypoint[10]
            cv2.line(annotated_frame, (int(p6[0]), int(p6[1])),
                        (int(p8[0]), int(p8[1])),
                        colors[track_id % len(colors)], 2)
            cv2.line(annotated_frame, (int(p8[0]), int(p8[1])),
                        (int(p10[0]), int(p10[1])),
                        colors[track_id % len(colors)], 2)


            # # 绘制关键点和关键点对应的序号
            # for j, point in enumerate(keypoint):
            #     cv2.circle(annotated_frame, (int(point[0]), int(point[1])), 1,
            #                colors[track_id % len(colors)], -1)
            #     cv2.putText(annotated_frame, str(j), (int(point[0]), int(point[1])),
            #                 cv2.FONT_HERSHEY_SIMPLEX, 0.5, colors[track_id % len(colors)], 1)



    return annotated_frame

def add_text_to_frame(strMsg, x, y, font, frame):
    # 将 OpenCV 图像帧转换为 PIL 图像
    frame_pil = Image.fromarray(cv2.cvtColor(frame, cv2.COLOR_BGR2RGB))
    draw = ImageDraw.Draw(frame_pil)

    # 输出中文文字
    draw.text((x, y), strMsg, font=font, fill=(255, 255, 255))

    # 将 PIL 图像转换回 OpenCV 图像帧
    frame_with_text = cv2.cvtColor(np.array(frame_pil), cv2.COLOR_RGB2BGR)
    
    return frame_with_text



ExitFlag = False 
video_path = 'E:\BaiduSyncdisk\PY\DataSet\视频\9月14日.mp4'
USBCam=0
RTSP='rtsp://guest:123456@10.103.8.236:554/avstream/channel=1/stream=1.sdp'
# 定义变量来计算帧率
fps_start_time = time.time() 
fps_counter = 0
fps_text=""
fps=30

# 创建一个标志变量，用于控制暂停和继续
paused = True
advance = False
reverse = False
record=False
recording=False
live=False



# 定义变量来保存当前视频的时间位置，单位为毫秒
current_time = 0
# 定义回退和前进的时间间隔，单位为毫秒
skip_interval = 10000  # 10秒

font = ImageFont.truetype('msyh.ttc', 24)

dataset = ThreadSafeDataset(max_frames=100)
# 创建读取视频线程
thread_reader = threading.Thread(target=video_reader_thread, args=(RTSP, dataset))
thread_reader.start()


# 创建 YOLOv8 姿势检测线程
thread_yolo_detection = threading.Thread(target=yolo_detection_thread, args=(dataset,))
thread_yolo_detection.start()


while not ExitFlag:

    start_time = time.time()   # 记录循环开始时间

    detect_count=0


    allframes=dataset.get_frames()
    if len(allframes) != 0:
        # 遍历每个帧数据并输出简图
        strMsg="["        
        count=0
        for frame_data in allframes:
            processed_frame = frame_data['processed_frame']
            if processed_frame is not None:
                strMsg+="#"
                detect_count=detect_count+1
            else:
                strMsg+="-"
            count=count+1
            if count % fps==0:
                strMsg+="]["

        strMsg+="]"
        print(strMsg)


    #如果paused为flase则运行以下代码
    if not paused:
        
        frames = dataset.get_frames()
    
        if len(frames) > 0:
            processed_frame = None

            if live is True:
                processed_frame=frames[-1]['frame']
            else:
                for i in range(len(frames) - 1, -1, -1):
                    frame_data = frames[i]
                    if 'processed_frame' in frame_data and frame_data['processed_frame'] is not None:
                        processed_frame = frame_data['processed_frame']
                        break
            
                if processed_frame is None:
                    latest_frame = frames[-1]
                    processed_frame= latest_frame['frame']

            # 计算并显示帧率
            # fps_counter += 1
            # if time.time()  - fps_start_time >= 1.0:
            #     display_fps = fps_counter / (time.time()  - fps_start_time)
            #     #print("FPS:", round(display_fps, 2))
            #     fps_text = f"FPS: {round(display_fps, 2)}"                 
            #     fps_start_time = time.time() 
            #     fps_counter = 0
            detect_fps=fps*detect_count/100
            fps_text=f"FPS:{round(detect_fps,2)}"        
            cv2.putText(processed_frame, fps_text, (0, 30), cv2.FONT_HERSHEY_SIMPLEX, 0.75, (0, 255, 0), 2)
            
            processed_frame = add_text_to_frame('a=退回10秒，d=前进10秒，r=录制，l=实时', 0,450, font, processed_frame)


            if recording ==True:
                #在processed_frame左上角显示红点
                cv2.circle(processed_frame, (10, 10), 5, (0, 0, 255), -1)

        
            cv2.imshow('YoloV8 Detect Post Frame', processed_frame)


    key = cv2.waitKey(1) & 0xFF

    if key == 27:
        # ESC
        ExitFlag = True

    if key == 32: # 空格键ASCII码是32
        #paused取反
        paused = not paused
        print("按了空格键")

    if key == 100 or key==68: # d键ASCII码是100  
        advance = True
        print("按了d键")

    if key == 97 or key==65:
        reverse = True 
        print("按了a键")

    if key==114 or key==82:
        record= not record
        print("按了r键")

    if key==108 or key==76:
        live=not live
        print("按了l键")


    end_time = time.time()  # 记录循环结束时间
    elapsed_time = end_time - start_time  # 计算循环执行时间

    # 计算动态延时
    frame_delay = 1 / fps - elapsed_time
    if frame_delay > 0:
        # print("耗时：",round(elapsed_time,5),"延时：", round(frame_delay, 5))
        time.sleep(frame_delay)



cv2.destroyAllWindows()

# 等待线程结束
# thread_reader.join()
# thread_display.join()
# thread_yolo_detection.join()