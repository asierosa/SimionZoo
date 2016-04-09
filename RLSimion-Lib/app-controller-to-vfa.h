#pragma once
#include "app.h"

class CLinearStateVFA;
class CController;
class CDirFileOutput;

class ControllerToVFAApp : public CApp
{
	CDirFileOutput *m_pOutputDirFile;
	CController* m_pController;
	int m_numVFAs;
	CLinearStateVFA** m_pVFAs;
public:

	ControllerToVFAApp(CParameters* pParameters,int argc,char* argv[]);
	~ControllerToVFAApp();

	void run();
};