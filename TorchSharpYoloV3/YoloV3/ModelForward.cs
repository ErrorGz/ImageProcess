namespace TorchSharpYoloV3.YoloV3
{
    using TorchSharp;
    using static TorchSharp.torch;
    internal partial class Model
    {

        public override Tensor forward(Tensor t)
        {
            Tensor b1 = block1.forward(t);
            Tensor b2 = block2.forward(b1);
            Tensor b3 = block3.forward(b2);
            Tensor b4 = block4.forward(b3);
            Tensor b5 = b2 + b4;////----------------//
            Tensor b6 = block6.forward(b5);
            Tensor b7 = block7.forward(b6);
            Tensor b8 = block8.forward(b7);
            Tensor b9 = b6 + b8;////----------------//
            Tensor b10 = block10.forward(b9);
            Tensor b11 = block11.forward(b10);
            Tensor b12 = b9 + b11;///--------------////
            Tensor b13 = block13.forward(b12);
            Tensor b14 = block14.forward(b13);
            Tensor b15 = block15.forward(b14);
            Tensor b16 = b13 + b15;///-------------//
            Tensor b17 = block17.forward(b16);
            Tensor b18 = block18.forward(b17);
            Tensor b19 = b16 + b18;///-------------////
            Tensor b20 = block20.forward(b19);
            Tensor b21 = block21.forward(b20);
            Tensor b22 = b19 + b21;///-------------//////
            Tensor b23 = block23.forward(b22);
            Tensor b24 = block24.forward(b23);
            Tensor b25 = b22 + b24;///-------------////////
            Tensor b26 = block26.forward(b25);
            Tensor b27 = block27.forward(b26);
            Tensor b28 = b25 + b27;///-------------//////////
            Tensor b29 = block29.forward(b28);
            Tensor b30 = block30.forward(b29);
            Tensor b31 = b28 + b30;///-------------////////////
            Tensor b32 = block32.forward(b31);
            Tensor b33 = block33.forward(b32);
            Tensor b34 = b31 + b33;///-------------//////////////
            Tensor b35 = block35.forward(b34);
            Tensor b36 = block36.forward(b35);
            Tensor b37 = b34 + b36;///-------------////////////////
            Tensor b38 = block38.forward(b37);
            Tensor b39 = block39.forward(b38);
            Tensor b40 = block40.forward(b39);
            Tensor b41 = b38 + b40;///-------------//
            Tensor b42 = block42.forward(b41);
            Tensor b43 = block43.forward(b42);
            Tensor b44 = b41 + b43;///-------------////
            Tensor b45 = block45.forward(b44);
            Tensor b46 = block46.forward(b45);
            Tensor b47 = b44 + b46;///-------------//////
            Tensor b48 = block48.forward(b47);
            Tensor b49 = block49.forward(b48);
            Tensor b50 = b47 + b49;///-------------////////
            Tensor b51 = block51.forward(b50);
            Tensor b52 = block52.forward(b51);
            Tensor b53 = b50 + b52;///-------------//////////
            Tensor b54 = block54.forward(b53);
            Tensor b55 = block55.forward(b54);
            Tensor b56 = b53 + b55;///-------------////////////
            Tensor b57 = block57.forward(b56);
            Tensor b58 = block58.forward(b57);
            Tensor b59 = b56 + b58;///-------------//////////////
            Tensor b60 = block60.forward(b59);
            Tensor b61 = block61.forward(b60);
            Tensor b62 = b59 + b61;///-------------////////////////
            Tensor b63 = block63.forward(b62);
            Tensor b64 = block64.forward(b63);
            Tensor b65 = block65.forward(b64);
            Tensor b66 = b63 + b65;///-------------//
            Tensor b67 = block67.forward(b66);
            Tensor b68 = block68.forward(b67);
            Tensor b69 = b66 + b68;///-------------////
            Tensor b70 = block70.forward(b69);
            Tensor b71 = block71.forward(b70);
            Tensor b72 = b69 + b71;///-------------//////
            Tensor b73 = block73.forward(b72);
            Tensor b74 = block74.forward(b73);
            Tensor b75 = b72 + b74;///-------------////////
            Tensor b76 = block76.forward(b75);
            Tensor b77 = block77.forward(b76);
            Tensor b78 = block78.forward(b77);
            Tensor b79 = block79.forward(b78);
            Tensor b80 = block80.forward(b79);
            Tensor b81 = block81.forward(b80);
            Tensor b82 = block82.forward(b81);
            // prediction scale 1
            Tensor b83
                = torch.permute(
                    b82.view(
                        b82.size(0),
                        num_anchors,
                        5 + num_classes,
                        b82.size(2), b82.size(2)),
                    new long[] { 0, 1, 3, 4, 2 }).contiguous();

            Tensor b84 = b80;
            Tensor b85 = block85.forward(b84);
            Tensor b86 = block86.forward(b85);
            Tensor b87 = torch.cat(new List<Tensor>() { b86, b60 }, 1);
            Tensor b88 = block88.forward(b87);
            Tensor b89 = block89.forward(b88);
            Tensor b90 = block90.forward(b89);
            Tensor b91 = block91.forward(b90);
            Tensor b92 = block92.forward(b91);
            Tensor b93 = block93.forward(b92);
            Tensor b94 = block94.forward(b93);
            // prediction scale 2
            Tensor b95
                = torch.permute(
                    b94.view(
                        b94.size(0),
                        num_anchors,
                        5 + num_classes,
                        b94.size(2), b94.size(2)),
                    new long[] { 0, 1, 3, 4, 2 }).contiguous();
            Tensor b96 = b92;
            Tensor b97 = block97.forward(b96);
            Tensor b98 = block98.forward(b97);
            Tensor b99 = torch.cat(new List<Tensor>() { b98, b35 }, 1);
            Tensor b100 = block100.forward(b99);
            Tensor b101 = block101.forward(b100);
            Tensor b102 = block102.forward(b101);
            Tensor b103 = block103.forward(b102);
            Tensor b104 = block104.forward(b103);
            Tensor b105 = block105.forward(b104);
            Tensor b106 = block106.forward(b105);
            // prediction scale 3
            Tensor b107
                = torch.permute(
                    b106.view(
                        b106.size(0),
                        num_anchors,
                        5 + num_classes,
                        b106.size(2), b106.size(2)),
                    new long[] { 0, 1, 3, 4, 2 }).contiguous();


            if (jump_start)
                samples = (new List<Tensor>() { b83, b95, b107 }).ToArray();


            return cat(new List<Tensor>() { b83.flatten(), b95.flatten(), b107.flatten() }, 0);
        }



    }
}
