namespace TorchSharpYoloV3.YoloV3
{
    using static TorchSharp.torch;
    using static TorchSharp.torch.nn;

    internal partial class Model
    {
        // =================================================
        // Block 1
        
        Module block1 = nn.Sequential(
                ("conv2d_001", nn.Conv2d(3, 32, 3, 1, 1)),
                ("bnrm2d_001", nn.BatchNorm2d(32)),
                ("reluLk_001", nn.LeakyReLU(0.1, true))
        );
        
        // =================================================
        // Block 2
        Module block2 = nn.Sequential(
                ("conv2d_002", nn.Conv2d(32, 64, 3, 2, 1)),
                ("bnrm2d_002", nn.BatchNorm2d(64)),
                ("reluLk_002", nn.LeakyReLU(0.1, true))
        );
       
        // =================================================
        // Block 3
        Module block3 = nn.Sequential(
                ("conv2d_003", nn.Conv2d(64, 32, 1, 1, 0)),
                ("bnrm2d_003", nn.BatchNorm2d(32)),
                ("reluLk_003", nn.LeakyReLU(0.1, true))
        );
        
        // =================================================
        // Block 4
        Module block4 = nn.Sequential(
                ("conv2d_004", nn.Conv2d(32, 64, 3, 1, 1)),
                ("bnrm2d_004", nn.BatchNorm2d(64)),
                ("reluLk_004", nn.LeakyReLU(0.1, true))
        );        
        // =================================================
        // Block 5
        // 
        // Shortcut -3
        // block2 + block4

        // =================================================
        // Block 6
        Module block6 = nn.Sequential(
                ("conv2d_006", nn.Conv2d(64, 128, 3, 2, 1)),
                ("bnrm2d_006", nn.BatchNorm2d(128)),
                ("reluLk_006", nn.LeakyReLU(0.1, true))
        );

        // =================================================
        // Block 7
        Module block7 = nn.Sequential(
                ("conv2d_007", nn.Conv2d(128, 64, 1, 1, 0)),
                ("bnrm2d_007", nn.BatchNorm2d(64)),
                ("reluLk_007", nn.LeakyReLU(0.1, true))
        );

        // =================================================
        // Block 8
        Module block8 = nn.Sequential(
                ("conv2d_008", nn.Conv2d(64, 128, 3, 1, 1)),
                ("bnrm2d_008", nn.BatchNorm2d(128)),
                ("reluLk_008", nn.LeakyReLU(0.1, true))
        );
        // =================================================
        // Block 9
        // 
        // Shortcut -3
        // block6 + block8

        // =================================================
        // Block 10
        Module block10 = nn.Sequential(
                ("conv2d_010", nn.Conv2d(128, 64, 1, 1, 0)),
                ("bnrm2d_010", nn.BatchNorm2d(64)),
                ("reluLk_010", nn.LeakyReLU(0.1, true))
        );

        // =================================================
        // Block 11
        Module block11 = nn.Sequential(
                ("conv2d_011", nn.Conv2d(64, 128, 3, 1, 1)),
                ("bnrm2d_011", nn.BatchNorm2d(128)),
                ("reluLk_011", nn.LeakyReLU(0.1, true))
        );
        // =================================================
        // Block 12
        // 
        // Shortcut -3
        // block6 + block8 + block11
        // =================================================
        // Block 13
        Module block13 = nn.Sequential(
                ("conv2d_013", nn.Conv2d(128, 256, 3, 2, 1)),
                ("bnrm2d_013", nn.BatchNorm2d(256)),
                ("reluLk_013", nn.LeakyReLU(0.1, true))
        );

        // =================================================
        // Block 14
        Module block14 = nn.Sequential(
                ("conv2d_014", nn.Conv2d(256, 128, 1, 1, 0)),
                ("bnrm2d_014", nn.BatchNorm2d(128)),
                ("reluLk_014", nn.LeakyReLU(0.1, true))
        );

        // =================================================
        // Block 15
        Module block15 = nn.Sequential(
                ("conv2d_015", nn.Conv2d(128, 256, 3, 1, 1)),
                ("bnrm2d_015", nn.BatchNorm2d(256)),
                ("reluLk_015", nn.LeakyReLU(0.1, true))
        );
        // =================================================
        // Block 16
        // 
        // Shortcut -3
        // block13 + block15

        // =================================================
        // Block 17
        Module block17 = nn.Sequential(
                ("conv2d_017", nn.Conv2d(256, 128, 1, 1, 0)),
                ("bnrm2d_017", nn.BatchNorm2d(128)),
                ("reluLk_017", nn.LeakyReLU(0.1, true))
        );

        // =================================================
        // Block 18
        Module block18 = nn.Sequential(
                ("conv2d_018", nn.Conv2d(128, 256, 3, 1, 1)),
                ("bnrm2d_018", nn.BatchNorm2d(256)),
                ("reluLk_018", nn.LeakyReLU(0.1, true))
        );
        // =================================================
        // Block 19
        // 
        // Shortcut -3
        // block13 + block15 + block18
        // =================================================
        // Block 20
        Module block20 = nn.Sequential(
                ("conv2d_020", nn.Conv2d(256, 128, 1, 1, 0)),
                ("bnrm2d_020", nn.BatchNorm2d(128)),
                ("reluLk_020", nn.LeakyReLU(0.1, true))
        );

        // =================================================
        // Block 21
        Module block21 = nn.Sequential(
                ("conv2d_021", nn.Conv2d(128, 256, 3, 1, 1)),
                ("bnrm2d_021", nn.BatchNorm2d(256)),
                ("reluLk_021", nn.LeakyReLU(0.1, true))
        );
        // =================================================
        // Block 22
        // 
        // Shortcut -3
        // block13 + block15 + block18 + block21
        // =================================================
        // Block 23
        Module block23 = nn.Sequential(
                ("conv2d_023", nn.Conv2d(256, 128, 1, 1, 0)),
                ("bnrm2d_023", nn.BatchNorm2d(128)),
                ("reluLk_023", nn.LeakyReLU(0.1, true))
        );
        // =================================================
        // Block 24
        Module block24 = nn.Sequential(
                ("conv2d_024", nn.Conv2d(128, 256, 3, 1, 1)),
                ("bnrm2d_024", nn.BatchNorm2d(256)),
                ("reluLk_024", nn.LeakyReLU(0.1, true))
        );
        // =================================================
        // Block 25
        // 
        // Shortcut -3
        // block13 + block15 + block18 + block21 + block24
        // =================================================
        // Block 26
        Module block26 = nn.Sequential(
                ("conv2d_026", nn.Conv2d(256, 128, 1, 1, 0)),
                ("bnrm2d_026", nn.BatchNorm2d(128)),
                ("reluLk_026", nn.LeakyReLU(0.1, true))
        );        
        // =================================================
        // Block 27
        Module block27 = nn.Sequential(
                ("conv2d_027", nn.Conv2d(128, 256, 3, 1, 1)),
                ("bnrm2d_027", nn.BatchNorm2d(256)),
                ("reluLk_027", nn.LeakyReLU(0.1, true))
        );
        // =================================================
        // Block 28
        // 
        // Shortcut -3
        // block13 + block15 + block18 + block21 + block24 +
        // block27
        // =================================================
        // Block 29
        Module block29 = nn.Sequential(
                ("conv2d_029", nn.Conv2d(256, 128, 1, 1, 0)),
                ("bnrm2d_029", nn.BatchNorm2d(128)),
                ("reluLk_029", nn.LeakyReLU(0.1, true))
        );
        // =================================================
        // Block 30
        Module block30 = nn.Sequential(
                ("conv2d_030", nn.Conv2d(128, 256, 3, 1, 1)),
                ("bnrm2d_030", nn.BatchNorm2d(256)),
                ("reluLk_030", nn.LeakyReLU(0.1, true))
        );
        // =================================================
        // Block 31
        // 
        // Shortcut -3
        // block13 + block15 + block18 + block21 + block24 +
        // block27 + block30
        // =================================================
        // Block 32
        Module block32 = nn.Sequential(
                ("conv2d_032", nn.Conv2d(256, 128, 1, 1, 0)),
                ("bnrm2d_032", nn.BatchNorm2d(128)),
                ("reluLk_032", nn.LeakyReLU(0.1, true))
        );
        // =================================================
        // Block 33
        Module block33 = nn.Sequential(
                ("conv2d_033", nn.Conv2d(128, 256, 3, 1, 1)),
                ("bnrm2d_033", nn.BatchNorm2d(256)),
                ("reluLk_033", nn.LeakyReLU(0.1, true))
        );
        // =================================================
        // Block 34
        // 
        // Shortcut -3
        // block13 + block15 + block18 + block21 + block24 +
        // block27 + block30 + block33
        // =================================================
        // Block 35
        Module block35 = nn.Sequential(
                ("conv2d_035", nn.Conv2d(256, 128, 1, 1, 0)),
                ("bnrm2d_035", nn.BatchNorm2d(128)),
                ("reluLk_035", nn.LeakyReLU(0.1, true))
        );
        // =================================================
        // Block 36
        Module block36 = nn.Sequential(
                ("conv2d_036", nn.Conv2d(128, 256, 3, 1, 1)),
                ("bnrm2d_036", nn.BatchNorm2d(256)),
                ("reluLk_036", nn.LeakyReLU(0.1, true))
        );
        // =================================================
        // Block 37
        // 
        // Shortcut -3
        // block13 + block15 + block18 + block21 + block24 +
        // block27 + block30 + block33 + block36
        // =================================================
        // Block 38
        Module block38 = nn.Sequential(
                ("conv2d_038", nn.Conv2d(256, 512, 3, 2, 1)),
                ("bnrm2d_038", nn.BatchNorm2d(512)),
                ("reluLk_038", nn.LeakyReLU(0.1, true))
        );
        // =================================================
        // Block 39
        Module block39 = nn.Sequential(
                ("conv2d_039", nn.Conv2d(512, 256, 1, 1, 0)),
                ("bnrm2d_039", nn.BatchNorm2d(256)),
                ("reluLk_039", nn.LeakyReLU(0.1, true))
        );
        // =================================================
        // Block 40
        Module block40 = nn.Sequential(
                ("conv2d_040", nn.Conv2d(256, 512, 3, 1, 1)),
                ("bnrm2d_040", nn.BatchNorm2d(512)),
                ("reluLk_040", nn.LeakyReLU(0.1, true))
        );
        // =================================================
        // Block 41
        // 
        // Shortcut -3
        // block38 + block40
        // =================================================
        // Block 42
        Module block42 = nn.Sequential(
                ("conv2d_042", nn.Conv2d(512, 256, 1, 1, 0)),
                ("bnrm2d_042", nn.BatchNorm2d(256)),
                ("reluLk_042", nn.LeakyReLU(0.1, true))
        );
        // =================================================
        // Block 43
        Module block43 = nn.Sequential(
                ("conv2d_043", nn.Conv2d(256, 512, 3, 1, 1)),
                ("bnrm2d_043", nn.BatchNorm2d(512)),
                ("reluLk_043", nn.LeakyReLU(0.1, true))
        );
        // =================================================
        // Block 44
        // 
        // Shortcut -3
        // block38 + block40 + block43
        // =================================================
        // Block 45
        Module block45 = nn.Sequential(
                ("conv2d_045", nn.Conv2d(512, 256, 1, 1, 0)),
                ("bnrm2d_045", nn.BatchNorm2d(256)),
                ("reluLk_045", nn.LeakyReLU(0.1, true))
        );
        // =================================================
        // Block 46
        Module block46 = nn.Sequential(
                ("conv2d_046", nn.Conv2d(256, 512, 3, 1, 1)),
                ("bnrm2d_046", nn.BatchNorm2d(512)),
                ("reluLk_046", nn.LeakyReLU(0.1, true))
        );
        // =================================================
        // Block 47
        // 
        // Shortcut -3
        // block38 + block40 + block43 + block46
        // =================================================
        // Block 48
        Module block48 = nn.Sequential(
                ("conv2d_048", nn.Conv2d(512, 256, 1, 1, 0)),
                ("bnrm2d_048", nn.BatchNorm2d(256)),
                ("reluLk_048", nn.LeakyReLU(0.1, true))
        );
        // =================================================
        // Block 49
        Module block49 = nn.Sequential(
                ("conv2d_049", nn.Conv2d(256, 512, 3, 1, 1)),
                ("bnrm2d_049", nn.BatchNorm2d(512)),
                ("reluLk_049", nn.LeakyReLU(0.1, true))
        );
        // =================================================
        // Block 50
        // 
        // Shortcut -3
        // block38 + block40 + block43 + block46 + block49
        // =================================================
        // Block 51
        Module block51 = nn.Sequential(
                ("conv2d_051", nn.Conv2d(512, 256, 1, 1, 0)),
                ("bnrm2d_051", nn.BatchNorm2d(256)),
                ("reluLk_051", nn.LeakyReLU(0.1, true))
        );
        // =================================================
        // Block 52
        Module block52 = nn.Sequential(
                ("conv2d_052", nn.Conv2d(256, 512, 3, 1, 1)),
                ("bnrm2d_052", nn.BatchNorm2d(512)),
                ("reluLk_052", nn.LeakyReLU(0.1, true))
        );
        // =================================================
        // Block 53
        // 
        // Shortcut -3
        // block38 + block40 + block43 + block46 + block49 +
        // block52
        // =================================================
        // Block 54
        Module block54 = nn.Sequential(
                ("conv2d_054", nn.Conv2d(512, 256, 1, 1, 0)),
                ("bnrm2d_054", nn.BatchNorm2d(256)),
                ("reluLk_054", nn.LeakyReLU(0.1, true))
        );
        // =================================================
        // Block 55
        Module block55 = nn.Sequential(
                ("conv2d_055", nn.Conv2d(256, 512, 3, 1, 1)),
                ("bnrm2d_055", nn.BatchNorm2d(512)),
                ("reluLk_055", nn.LeakyReLU(0.1, true))
        );
        // =================================================
        // Block 56
        // 
        // Shortcut -3
        // block38 + block40 + block43 + block46 + block49 +
        // block52 + block55
        // =================================================
        // Block 57
        Module block57 = nn.Sequential(
                ("conv2d_057", nn.Conv2d(512, 256, 1, 1, 0)),
                ("bnrm2d_057", nn.BatchNorm2d(256)),
                ("reluLk_057", nn.LeakyReLU(0.1, true))
        );
        // =================================================
        // Block 58
        Module block58 = nn.Sequential(
                ("conv2d_058", nn.Conv2d(256, 512, 3, 1, 1)),
                ("bnrm2d_058", nn.BatchNorm2d(512)),
                ("reluLk_058", nn.LeakyReLU(0.1, true))
        );
        // =================================================
        // Block 59
        // 
        // Shortcut -3
        // block38 + block40 + block43 + block46 + block49 +
        // block52 + block55 + block58
        // =================================================
        // Block 60
        Module block60 = nn.Sequential(
                ("conv2d_060", nn.Conv2d(512, 256, 1, 1, 0)),
                ("bnrm2d_060", nn.BatchNorm2d(256)),
                ("reluLk_060", nn.LeakyReLU(0.1, true))
        );
        // =================================================
        // Block 61
        Module block61 = nn.Sequential(
                ("conv2d_061", nn.Conv2d(256, 512, 3, 1, 1)),
                ("bnrm2d_061", nn.BatchNorm2d(512)),
                ("reluLk_061", nn.LeakyReLU(0.1, true))
        );
        // =================================================
        // Block 62
        // 
        // Shortcut -3
        // block38 + block40 + block43 + block46 + block49 +
        // block52 + block55 + block58 + block61
        // =================================================
        // Block 63
        Module block63 = nn.Sequential(
                ("conv2d_063", nn.Conv2d(512, 1024, 3, 2, 1)),
                ("bnrm2d_063", nn.BatchNorm2d(1024)),
                ("reluLk_063", nn.LeakyReLU(0.1, true))
        );
        // =================================================
        // Block 64
        Module block64 = nn.Sequential(
                ("conv2d_064", nn.Conv2d(1024, 512, 1, 1, 0)),
                ("bnrm2d_064", nn.BatchNorm2d(512)),
                ("reluLk_064", nn.LeakyReLU(0.1, true))
        );
        // =================================================
        // Block 65
        Module block65 = nn.Sequential(
                ("conv2d_065", nn.Conv2d(512, 1024, 3, 1, 1)),
                ("bnrm2d_065", nn.BatchNorm2d(1024)),
                ("reluLk_065", nn.LeakyReLU(0.1, true))
        );
        // =================================================
        // Block 66
        // 
        // Shortcut -3
        // block63 + block65
        // =================================================
        // Block 67
        Module block67 = nn.Sequential(
                ("conv2d_067", nn.Conv2d(1024, 512, 1, 1, 0)),
                ("bnrm2d_067", nn.BatchNorm2d(512)),
                ("reluLk_067", nn.LeakyReLU(0.1, true))
        );
        // =================================================
        // Block 68
        Module block68 = nn.Sequential(
                ("conv2d_068", nn.Conv2d(512, 1024, 3, 1, 1)),
                ("bnrm2d_068", nn.BatchNorm2d(1024)),
                ("reluLk_068", nn.LeakyReLU(0.1, true))
        );
        // =================================================
        // Block 69
        // 
        // Shortcut -3
        // block63 + block65 + block68
        // =================================================
        // Block 70
        Module block70 = nn.Sequential(
                ("conv2d_070", nn.Conv2d(1024, 512, 1, 1, 0)),
                ("bnrm2d_070", nn.BatchNorm2d(512)),
                ("reluLk_070", nn.LeakyReLU(0.1, true))
        );
        // =================================================
        // Block 71
        Module block71 = nn.Sequential(
                ("conv2d_071", nn.Conv2d(512, 1024, 3, 1, 1)),
                ("bnrm2d_071", nn.BatchNorm2d(1024)),
                ("reluLk_071", nn.LeakyReLU(0.1, true))
        );
        // =================================================
        // Block 72
        // 
        // Shortcut -3
        // block63 + block65 + block68 + block71
        // =================================================
        // Block 73
        Module block73 = nn.Sequential(
                ("conv2d_073", nn.Conv2d(1024, 512, 1, 1, 0)),
                ("bnrm2d_073", nn.BatchNorm2d(512)),
                ("reluLk_073", nn.LeakyReLU(0.1, true))
        );
        // =================================================
        // Block 74
        Module block74 = nn.Sequential(
                ("conv2d_074", nn.Conv2d(512, 1024, 3, 1, 1)),
                ("bnrm2d_074", nn.BatchNorm2d(1024)),
                ("reluLk_074", nn.LeakyReLU(0.1, true))
        );
        // =================================================
        // Block 75
        // 
        // Shortcut -3
        // block63 + block65 + block68 + block71 + block74
        // =================================================
        // Block 76
        Module block76 = nn.Sequential(
                ("conv2d_076", nn.Conv2d(1024, 512, 1, 1, 0)),
                ("bnrm2d_076", nn.BatchNorm2d(512)),
                ("reluLk_076", nn.LeakyReLU(0.1, true))
        );
        // =================================================
        // Block 77
        Module block77 = nn.Sequential(
                ("conv2d_077", nn.Conv2d(512, 1024, 3, 1, 1)),
                ("bnrm2d_077", nn.BatchNorm2d(1024)),
                ("reluLk_077", nn.LeakyReLU(0.1, true))
        );
        // =================================================
        // Block 78
        Module block78 = nn.Sequential(
                ("conv2d_078", nn.Conv2d(1024, 512, 1, 1, 0)),
                ("bnrm2d_078", nn.BatchNorm2d(512)),
                ("reluLk_078", nn.LeakyReLU(0.1, true))
        );
        // =================================================
        // Block 79
        Module block79 = nn.Sequential(
                ("conv2d_079", nn.Conv2d(512, 1024, 3, 1, 1)),
                ("bnrm2d_079", nn.BatchNorm2d(1024)),
                ("reluLk_079", nn.LeakyReLU(0.1, true))
        );
        // =================================================
        // Block 80
        Module block80 = nn.Sequential(
                ("conv2d_080", nn.Conv2d(1024, 512, 1, 1, 0)),
                ("bnrm2d_080", nn.BatchNorm2d(512)),
                ("reluLk_080", nn.LeakyReLU(0.1, true))
        );

        // =================================================
        // Block 81
        Module block81 = nn.Sequential(
                ("conv2d_081", nn.Conv2d(512, 1024, 3, 1, 1)),
                ("bnrm2d_081", nn.BatchNorm2d(1024)),
                ("reluLk_081", nn.LeakyReLU(0.1, true))
        );
        // =================================================
        // Block 82
        Module block82 = nn.Sequential(
                ("conv2d_082", nn.Conv2d(1024, 255, 1, 1, 0))
        );
        // =================================================
        // Block 83
        // 
        // Yolo
        // =================================================
        // Block 84
        // 
        // Route -4
        // block80
        // =================================================
        // Block 85
        Module block85 = nn.Sequential(
                ("conv2d_085", nn.Conv2d(512, 256, 1, 1, 0)),
                ("bnrm2d_085", nn.BatchNorm2d(256)),
                ("reluLk_085", nn.LeakyReLU(0.1, true))
        );
        // =================================================
        // Block 86
        Module block86 = nn.Upsample(scale_factor: new double[] { 2, 2 }, mode: UpsampleMode.Nearest);
        // =================================================
        // Block 87
        // 
        // Route -1, 60
        // cat(block86,block60)
        // =================================================
        // Block 88
        Module block88 = nn.Sequential(
                ("conv2d_088", nn.Conv2d(512, 256, 1, 1, 0)),
                ("bnrm2d_088", nn.BatchNorm2d(256)),
                ("reluLk_088", nn.LeakyReLU(0.1, true))
        );
        // =================================================
        // Block 89
        Module block89 = nn.Sequential(
                ("conv2d_089", nn.Conv2d(256, 512, 3, 1, 1)),
                ("bnrm2d_089", nn.BatchNorm2d(512)),
                ("reluLk_089", nn.LeakyReLU(0.1, true))
        );
        // =================================================
        // Block 90
        Module block90 = nn.Sequential(
                ("conv2d_090", nn.Conv2d(512, 256, 1, 1, 0)),
                ("bnrm2d_090", nn.BatchNorm2d(256)),
                ("reluLk_090", nn.LeakyReLU(0.1, true))
        );
        // =================================================
        // Block 91
        Module block91 = nn.Sequential(
                ("conv2d_091", nn.Conv2d(256, 512, 3, 1, 1)),
                ("bnrm2d_091", nn.BatchNorm2d(512)),
                ("reluLk_091", nn.LeakyReLU(0.1, true))
        );
        // =================================================
        // Block 92
        Module block92 = nn.Sequential(
                ("conv2d_092", nn.Conv2d(512, 256, 1, 1, 0)),
                ("bnrm2d_092", nn.BatchNorm2d(256)),
                ("reluLk_092", nn.LeakyReLU(0.1, true))
        );
        // =================================================
        // Block 93
        Module block93 = nn.Sequential(
                ("conv2d_093", nn.Conv2d(256, 512, 3, 1, 1)),
                ("bnrm2d_093", nn.BatchNorm2d(512)),
                ("reluLk_093", nn.LeakyReLU(0.1, true))
        );
        // =================================================
        // Block 94
        Module block94 = nn.Sequential(
                ("conv2d_094", nn.Conv2d(512, 255, 1, 1, 0))
        );
        // =================================================
        // Block 95
        // 
        // Yolo
        // =================================================
        // Block 96
        // 
        // Route -4
        // block92
        // =================================================
        // Block 97
        Module block97 = nn.Sequential(
                ("conv2d_097", nn.Conv2d(256, 128, 1, 1, 0)),
                ("bnrm2d_097", nn.BatchNorm2d(128)),
                ("reluLk_097", nn.LeakyReLU(0.1, true))
        );
        // =================================================
        // Block 98
        Module block98 = nn.Upsample(scale_factor: new double[] { 2, 2 }, mode: UpsampleMode.Nearest);
        // =================================================
        // Block 99
        // 
        // Route -1, 35
        // cat(block98,block35)
        // =================================================
        // Block 100
        Module block100 = nn.Sequential(
                ("conv2d_100", nn.Conv2d(256, 128, 1, 1, 0)),
                ("bnrm2d_100", nn.BatchNorm2d(128)),
                ("reluLk_100", nn.LeakyReLU(0.1, true))
        );
        // =================================================
        // Block 101
        Module block101 = nn.Sequential(
                ("conv2d_101", nn.Conv2d(128, 256, 3, 1, 1)),
                ("bnrm2d_101", nn.BatchNorm2d(256)),
                ("reluLk_101", nn.LeakyReLU(0.1, true))
        );
        // =================================================
        // Block 102
        Module block102 = nn.Sequential(
                ("conv2d_102", nn.Conv2d(256, 128, 1, 1, 0)),
                ("bnrm2d_102", nn.BatchNorm2d(128)),
                ("reluLk_102", nn.LeakyReLU(0.1, true))
        );
        // =================================================
        // Block 103
        Module block103 = nn.Sequential(
                ("conv2d_103", nn.Conv2d(128, 256, 3, 1, 1)),
                ("bnrm2d_103", nn.BatchNorm2d(256)),
                ("reluLk_103", nn.LeakyReLU(0.1, true))
        );
        // =================================================
        // Block 104
        Module block104 = nn.Sequential(
                ("conv2d_104", nn.Conv2d(256, 128, 1, 1, 0)),
                ("bnrm2d_104", nn.BatchNorm2d(128)),
                ("reluLk_104", nn.LeakyReLU(0.1, true))
        );
        // =================================================
        // Block 105
        Module block105 = nn.Sequential(
                ("conv2d_105", nn.Conv2d(128, 256, 3, 1, 1)),
                ("bnrm2d_105", nn.BatchNorm2d(256)),
                ("reluLk_105", nn.LeakyReLU(0.1, true))
        );
        // =================================================
        // Block 106
        Module block106 = nn.Sequential(
                ("conv2d_106", nn.Conv2d(256, 255, 1, 1, 0))
        );
        // =================================================
        // Block 107
        // 
        // Yolo
    }



}
