include $(APSIM)/Model/Build/Platform.make

PROJECT = ApsimShared

SRC =   ../General/pch.cpp \
    ApsimShared.cpp \
	ApsimComponentData.cpp  \
	ApsimControlFile.cpp \
	ApsimDirectories.cpp \
	ApsimServiceData.cpp \
	ApsimSettings.cpp \
	ApsimSimulationFile.cpp \
	ApsimSystemData.cpp \
	ApsimVersion.cpp  \
	ApsimDataFile.cpp \
	ApsimDataTypesFile.cpp \
	ApsimDataTypeData.cpp \
	ApsimRegistry.cpp \
	ApsimRegistration.cpp \
	ApsimRegistrationType.cpp \
	ApsimRegistrationData.cpp \
	SimCreator.cpp \
	SimCreatorSection.cpp \
	ApsimMacroFile.cpp \
	FString.cpp \
	FStringExt.cpp \
	Phi_functions.cpp

ifeq ($(PLATFORM),Linux)
DEF = ApsimShared.ldscript
endif	
	
LIBS = General
PROJECTTYPE = libdll

PRECOMPILE = ../General/pch.h

include $(APSIM)/Model/Build/$(PLATFORM)CPP.make
