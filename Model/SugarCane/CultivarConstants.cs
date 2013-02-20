﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


   public class CultivarConstants
        {


        const int max_table = 10;   //! Maximum size_of of tables
        const int max_leaf = 200;   //! maximum number of plant leaves

        //*     ===========================================================
        //      subroutine sugar_read_cultivar_params (section_name)
        //*     ===========================================================


        //!    sugar_leaf_size

        [Param(MinVal = 1000.0, MaxVal = 100000.0)]
        double[] leaf_size = new double[max_table];

        [Param(MinVal = 0.0, MaxVal = max_leaf)]
        double[] leaf_size_no = new double[max_table];



        [Param(MinVal = 0.0, MaxVal = 1.0)]
        double cane_fraction;


        [Param(MinVal = 0.0, MaxVal = 1.0)]
        double[] sucrose_fraction_stalk = new double[max_table];


        [Param(MinVal = 0.0, MaxVal = 1.0)]
        double[] stress_factor_stalk = new double[max_table];


        [Param(MinVal = 0.0, MaxVal = 2000.0)]
        double sucrose_delay;


        [Param(MinVal = 0.0, MaxVal = 5000.0)]
        [Units("g/m2")]
        double min_sstem_sucrose;


        [Param(MinVal = 0.0, MaxVal = 5000.0)]
        [Units("g/m2")]
        double min_sstem_sucrose_redn;

        //TODO: Either find a way to do this or remove 'tt_emerg_to_begcane_ub' from the ini file.
        //[Param(MinVal = 0.0, MaxVal = tt_emerg_to_begcane_ub)]
        [Param(MinVal = 0.0, MaxVal = 2000.0)]
        double tt_emerg_to_begcane;


        //TODO: Either find a way to do this or remove 'tt_begcane_to_flowering_ub' from the ini file.
        //[Param(MinVal = 0.0, MaxVal = tt_begcane_to_flowering_ub)]
        [Param(MinVal = 0.0, MaxVal = 10000.0)]
        double tt_begcane_to_flowering;


        //TODO: Either find a way to do this or remove 'tt_flowering_to_crop_end_ub' from the ini file.
        //[Param(MinVal = 0.0, MaxVal = tt_flowering_to_crop_end_ub)]
        [Param(MinVal = 0.0, MaxVal = 5000.0)]
        double tt_flowering_to_crop_end;



        //!    sugar_leaf_death

        [Param(MinVal = 0.0, MaxVal = max_leaf)]
        double green_leaf_no;


        //!    sugar_leaf_size

        [Param(MinVal = 0.0, MaxVal = 100.0)]
        double[] tillerf_leaf_size = new double[max_table];

        [Param(MinVal = 0.0, MaxVal = max_leaf)]
        double[] tillerf_leaf_size_no = new double[max_table];

        }
 