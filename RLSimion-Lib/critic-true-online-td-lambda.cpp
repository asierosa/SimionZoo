#include "stdafx.h"
#include "critic.h"
#include "vfa.h"
#include "parameters.h"
#include "features.h"
#include "globals.h"
#include "experiment.h"

CTrueOnlineTDLambdaCritic::CTrueOnlineTDLambdaCritic(CParameters *pParameters)
{
	m_pVFA= new CRBFFeatureGridVFA(pParameters->getStringPtr("SIMGOD/CRITIC/VALUE_FUNCTION_RBF_VARIABLES"));
	m_e= new CFeatureList();
	m_aux= new CFeatureList();
	m_v_s= 0.0;

	m_pAlpha= pParameters->add("SIMGOD/CRITIC/LEARNING_RATE",0.0);
	m_gamma= pParameters->getDouble("SIMGOD/CRITIC/INITIAL_GAMMA");
	m_lambda= pParameters->getDouble("SIMGOD/CRITIC/INITIAL_LAMBDA");

	if (pParameters->exists("SIMGOD/CRITIC/LOAD"))
		loadVFunction(pParameters->getStringPtr("SIMGOD/CRITIC/LOAD"));

	if (pParameters->exists("SIMGOD/CRITIC/SAVE"))
		strcpy_s(m_saveFilename,1024,pParameters->getStringPtr("SIMGOD/CRITIC/SAVE"));
	else
		m_saveFilename[0]= 0;
}

CTrueOnlineTDLambdaCritic::~CTrueOnlineTDLambdaCritic()
{
	if (m_saveFilename[0]!=0)
		saveVFunction(m_saveFilename);

	delete m_pVFA;
	delete m_e;
	delete m_aux;
}

double CTrueOnlineTDLambdaCritic::updateValue(CState *s, CAction *a, CState *s_p, double r,double rho)
{
	double v_s_p;

	if (*m_pAlpha==0.0) return 0.0;
	
	if (g_pExperiment->m_step==1)
	{
		m_e->clear();
		//vs= theta^T * phi(s)
		m_pVFA->getFeatures(s,0,m_aux);
		m_v_s= m_pVFA->getValue(m_aux);
	}
		
	//vs_p= theta^T * phi(s_p)
	m_pVFA->getFeatures(s_p,0,m_aux);
	v_s_p= m_pVFA->getValue(m_aux);

	//delta= R + gamma* v_s_p - v_s
	double td= r + m_gamma*v_s_p - m_v_s;

	//e= gamma*lambda*e + alpha*[1-gamma*lambda* e^T*phi(s)]* phi(s)
	m_pVFA->getFeatures(s,0,m_aux);										//m_aux <- phi(s)
	double e_T_phi_s= m_e->innerProduct(m_aux);


	m_e->mult(m_gamma*m_lambda);
	m_e->addFeatureList(m_aux,*m_pAlpha *(1-m_gamma*m_lambda*e_T_phi_s));
	m_e->applyThreshold(0.0001);	

	//theta= theta + delta*e + alpha[v_s - theta^T*phi(s)]* phi(s)
	m_pVFA->add(m_e,td);
	double theta_T_phi_s= m_pVFA->getValue(m_aux);
	m_pVFA->add(m_aux,*m_pAlpha *(m_v_s - theta_T_phi_s));
	//v_s= v_s_p
	m_v_s= v_s_p;

	return td;
}
