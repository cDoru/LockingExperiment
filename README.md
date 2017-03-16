# LockingExperiment
Experiment on using sql application-wide locks

This application implements a sql application-wide lock (look under valuescontroller) that basically allows you to execute one 
and only one action at a time based on parameters (you can protect resources that are not concurrent but are exposed through api calls this way)
