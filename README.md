# Solutions

Here you can find the solutions ran for this research. The tables below group solutions according to the model used (Costa and Miralles Model (2009) and Borba and Ritt Model (2014)) and the instance family (heskia, roszieg, tonge or wee-mag). Each instance family has exactly 80 instances, being 320 instances in total.

For each solution, there is an available file (you may inspect the file by clicking the link). The tables present the name of the instance, the number of periods, the maximum mean cycle time inputed, the number of workers, number of tasks and the objective function value.

<details><summary>Heskia - Costa and Miralles Model (2009)</summary>
<p>

Instance | Periods | Maximum Mean Cycle Time | Workers | Taks | OF
:------: | :-------: | :---: | :-------: | :---: | :---:
[1_hes](linkdooutput) | x | x | x | x | x

</p>
</details>


## References
<!--
R &nbsp; &mdash; &nbsp; Isaiah Reimer &nbsp; (isaiah `dot` reimer `at` rideco `dot` com) - [RideCo](https://rideco.com/)

SB &nbsp; &mdash; &nbsp; Carlo Sartori and Luciana Buriol. A study on the Pickup and Delivery Problem with Time Windows: Matheuristics and new instances. Available in [COR](https://doi.org/10.1016/j.cor.2020.105065)

Shobb &nbsp; &mdash; &nbsp; Shobb &nbsp; ( shobb `at` narod `dot` ru )

VRt &nbsp; &mdash; &nbsp; Dmitriy Demin, Mikhail Diakov (msd `at` veeroute `dot` com), Ivan Ilin, Nikita Ivanov, Viacheslav Sokolov (vs `at` veeroute `dot` com) et al. [VRt Global](https://veeroute.com/)

VACS &nbsp; &mdash; &nbsp; Simen T. Vadseth, Henrik Andersson, Jean-Francois Cordeau and Magnus Stålhane. To be announced.
-->
## File naming and structure

Be aware of the naming convention of the solution files. They should be named as:

```
output-<instance-name>-c<maximum-mean-cycle-time>-<model-used>-<extra-constraints-used>-<number-of-periods>.json
```
Each output file follows this structure:
```
{
  "NumberOfTasks": <integer>,
  "NumberOfWorkers": <integer>,
  "NumberOfPeriods": <integer>,
  "OriginalMaximumMeanCycleTime": <integer, original value inputed to run the project>,
  "MaximumMeanCycleTime": <integer, generated by using the fields "OriginalMaximumMeanCycleTime" x "PercentageAppliedOverOriginalMaximumMeanCycleTime", rounded floor>,
  "PercentageAppliedOverOriginalMaximumMeanCycleTime": <double, multiplier to be used over the "OriginalMaximumMeanCycleTime", 5% is written as 1.05>,
  "MeanCycleTime": <double, real mean cycle time returned by the algorithm>,
  "ExecutionTimeMs": <long, time taken by gurobi to reach the said solution>,
  "OF": <integer, the objective function, counting the variety of tasks executed by different workers>,
  "Assignment": {
    "Period x": {
      "Station x": {
        "Worker x": [ <integers, list of taks executed> ]
      }
    }
  },
  "NewTasksExecutedByWorkerOnEachPeriod": {
    "Period x": {
      "Station x": {
        "Worker x": [ <integers, list of taks newly executed, up until this period, this worker had never executed these tasks> ]
      }
    }
  }
}
```
