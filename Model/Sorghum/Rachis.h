//---------------------------------------------------------------------------

#ifndef RachisH
#define RachisH

#include "PlantComponents.h"
#include "Utilities.h"

namespace Sorghum {
//------------------------------------------------------------------------------------------------

class Rachis : public PlantPart
   {
   private:

// Parameters ----------------------------------------------------------

   // nitrogen
   double initialNConc;
   double targetNConc;
   double structRachisNConc;
   double dilnNSlope;
   double dilnNInt;

//  Variables  ----------------------------------------------------------


// Private Methods -------------------------------------------------------
   void  doRegistrations(void);
   void  initialize(void);

// public Methods -------------------------------------------------------
   public:
   Rachis(ScienceAPI2 &, Plant *p);
   ~Rachis();
   // plant
   void  readParams (void);
   void  updateVars(void);

   // biomass
   double partitionDM(double dltDM);

   // nitrogen
   double calcNDemand(void);
   double calcStructNDemand(void);
   double provideN(double requiredN);

   // phosphorus
   double calcPDemand(void);

   // phenology
   void  phenologyEvent(int);
  };


//---------------------------------------------------------------------------
}
#endif
