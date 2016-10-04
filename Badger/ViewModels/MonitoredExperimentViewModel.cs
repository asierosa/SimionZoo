﻿using Badger.Models;
using Badger.Data;
using System.Collections.ObjectModel;
using System.Xml;
using System.Windows;
using Caliburn.Micro;
using System.Windows.Forms;
using System.IO;
using System.Dynamic;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.IO.Pipes;
using System.Xml.Linq;
using System.Linq;
using System.Xml.XPath;
using System.Windows.Media;
using System.Threading;
using System.Globalization;
using System.Collections.Concurrent;
using Herd;
using Badger.ViewModels;


namespace Badger.ViewModels
{
    public class MonitoredExperimentViewModel : PropertyChangedBase
    {
        private string m_name;
        public string name { get { return m_name; } set { m_name = value; NotifyOfPropertyChange(() => name); } }

        public string pipeName { get { return m_name; } }

        private string m_filePath;
        public string filePath { get { return m_filePath; } set { m_filePath = value; } }

        private XmlDocument m_experimentXML;
        public XmlDocument experimentXML { get { return m_experimentXML; } set { m_experimentXML = value; } }


        //STATE
        public enum ExperimentState { RUNNING, FINISHED, ERROR, ENQUEUED, SENDING, RECEIVING, WAITING_EXECUTION, WAITING_RESULT };
        private ExperimentState m_state = ExperimentState.ENQUEUED;
        public ExperimentState state
        {
            get { return m_state; }
            set
            {
                //if a task within a job fails, we don't want to overwrite it's state when the job finishes
                //we don't update state in case new state is not RUNNING or SENDING
                if (m_state != ExperimentState.ERROR || value == ExperimentState.WAITING_EXECUTION)
                    m_state = value;
                NotifyOfPropertyChange(() => state);
                NotifyOfPropertyChange(() => isRunning);
                NotifyOfPropertyChange(() => stateString);
                NotifyOfPropertyChange(() => stateColor);
            }
        }

        public void resetState()
        {
            state = ExperimentState.ENQUEUED;
            NotifyOfPropertyChange(() => state);
        }
        public bool isRunning
        {
            get { return m_state == ExperimentState.RUNNING; }
            set { }
        }
        public bool isEnqueued
        {
            get { return m_state == ExperimentState.ENQUEUED; }
            set { }
        }

        public string stateString
        {
            get
            {
                switch (m_state)
                {
                    case ExperimentState.RUNNING: return "Running";
                    case ExperimentState.FINISHED: return "Finished";
                    case ExperimentState.ERROR: return "Error";
                    case ExperimentState.SENDING: return "Sending";
                    case ExperimentState.RECEIVING: return "Receiving";
                    case ExperimentState.WAITING_EXECUTION: return "Awaiting";
                    case ExperimentState.WAITING_RESULT: return "Awaiting";
                }
                return "";
            }
        }

        public string stateColor
        {
            get
            {
                switch (m_state)
                {
                    case ExperimentState.ENQUEUED:
                    case ExperimentState.RUNNING:
                    case ExperimentState.SENDING:
                    case ExperimentState.RECEIVING:
                    case ExperimentState.WAITING_EXECUTION:
                    case ExperimentState.WAITING_RESULT:
                        return "Black";
                    case ExperimentState.FINISHED: return "DarkGreen";
                    case ExperimentState.ERROR: return "Red";
                }
                return "Black";
            }
        }

        private double m_progress;
        public double progress
        {
            get { return m_progress; }
            set
            {
                m_progress = value; NotifyOfPropertyChange(() => progress);
            }
        }

        private string m_statusInfo;
        public string statusInfo
        {
            get { return m_statusInfo; }
            set
            {
                m_statusInfo = value;
                NotifyOfPropertyChange(() => statusInfo);
                //NotifyOfPropertyChange(() => isStatusInfo);
            }
        }
        public void addStatusInfoLine(string line)
        { statusInfo += line + "\n"; }

        //public bool isStatusInfo { get { return m_statusInfo != ""; } set { } }

        //log stuff
        private Logger.LogFunction m_logFunction = null;
        private void logMessage(string message)
        {
            m_logFunction?.Invoke(message);
        }

        public MonitoredExperimentViewModel(ExperimentViewModel experiment, PlotViewModel evaluationMonitorVM)
        {
            name = experiment.name;
            filePath = experiment.filePath;
            experimentXML= experiment.experimentXML;
            evaluationMonitor = evaluationMonitorVM;
        }

        //evaluation plot stuff
        private int evaluationSeriesId = -1;
        private PlotViewModel evaluationMonitor= null;
        public void addEvaluationValue(double xNorm, double y)
        {
            if (evaluationSeriesId == -1) //series not yet added
                evaluationSeriesId = evaluationMonitor.addLineSeries(name);
            evaluationMonitor.addLineSeriesValue(evaluationSeriesId, xNorm, y);
        }
    }
 }