using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Kinect;

namespace HumanVolumeCalculator
{
   public struct JointsCoo
    {
        public SkeletonPoint HeadCoo { get; set; }
        public SkeletonPoint SpineShoulderCoo { get; set; }
        public SkeletonPoint ShoulderLeftCoo { get; set; }
        public SkeletonPoint ShoulderRightCoo { get; set; }
        public SkeletonPoint ElbowRightCoo { get; set; }
        public SkeletonPoint WristRightCoo { get; set; }
        public SkeletonPoint HandRightCoo { get; set; }
        public SkeletonPoint ElbowLeftCoo { get; set; }
        public SkeletonPoint WirstLeftCoo { get; set; }
        public SkeletonPoint HandLeftCoo { get; set; }
        public SkeletonPoint SpineBaseCoo { get; set; }
        public SkeletonPoint HipRightCoo { get; set; }
        public SkeletonPoint KneeRightCoo { get; set; }
        public SkeletonPoint AnkleRightCoo { get; set; }
        public SkeletonPoint FootRightCoo { get; set; }
        public SkeletonPoint HipLeftCoo { get; set; }
        public SkeletonPoint KneeLeftCoo { get; set; }
        public SkeletonPoint AnkleLeftCoo { get; set; }
        public SkeletonPoint FootLeftCoo { get; set; }
        public SkeletonPoint SpineMidCoo { get; set; }
        public SkeletonPoint NeckCoo { get; set; }
        public SkeletonPoint HandTipLeftCoo { get; set; }
        public SkeletonPoint HandTipRightCoo { get; set; }
        public SkeletonPoint ThumbRightCoo { get; set; }
        public SkeletonPoint ThumbLeftCoo { get; set; }

    }
}
