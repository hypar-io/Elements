<style type="text/css">
img[src*='#center'] {
    display: block;
    margin: auto;
}
img[alt=center] {
margin:10
}
</style>

# Eccentric reducers creation

The idea of the algorithm is to add AdditionalTransform to the connections.

AdditionalTransform keeps the offset from the centerline (or the line where flow collection nodes are located).

Each reducer keeps the TrunkSideTransform and BranchSideTransform.

These values are used as AdditionalTransform form TrunkSideComponent and BranchSideComponents.

## Reducer on trunk side

![image](images\ReducerOnTrunkSide1.PNG)

1. Insert reducer on trunk side:

Trunk Side offset is 0 ![center](images\Reducer1Description.PNG) Branch Side offset is H


![image](images\ReducerOnTrunkSide2.PNG)

2. Add AdditionalTransform to the pipe and branch connection

![image](images\ReducerOnTrunkSide3.PNG)

## Reducer on branch side

![image](images\ReducerOnBranchSide1.PNG)

1. Insert reducer on branch side:

Trunk Side offset is H ![center](images\Reducer2Description.PNG) Branch Side offset is 0

![image](images\ReducerOnBranchSide2.PNG)

2. Add TrunkSideTransform.Inverted() to the Additionaltransform of reducer and branch side connection

![image](images\ReducerOnBranchSide3.PNG)

## The resize pipe functionality

![image](images\Resize1.PNG)

1. Resize pipe

![image](images\Resize2.PNG)

2. Insert reducer on Trunk side 

![image](images\Resize3.PNG)

3. Add AdditionalTransform to the pipe

![image](images\Resize4.PNG)

4. Insert reducer on the Branch side

![image](images\Resize5.PNG)

## Sample

1. Create pipes between 4 connections, one of them is Wye connection

![image](images\Sample1.PNG)
 
2. Create pipes between Terminal 1 and Wye

![image](images\Sample2.PNG)
 
3. Add reducer between Terminal 1 and Pipe Segment 1

![image](images\Sample3.PNG)
 
4. Apply Reducer.BranchSideTransform to Pipe Segment 1 and Wye connection

![image](images\Sample4.PNG)
 
5. Create pipe between Wye connection and Terminal 2

![image](images\Sample5.PNG)
 
6. Apply Wye AdditionalTransform to Pipe Segment 2 and Terminal 2 connection

![image](images\Sample6.PNG)
 
7. Create pipe between Wye connection and Terminal 3

![image](images\Sample7.PNG)

8. Apply Wye AdditionalTransform to Pipe Segment 3 and Terminal 3 connection

![image](images\Sample8.PNG)
 
9. Create reducer between Pipe Segment 3 and Terminal 3

![image](images\Sample9.PNG)
 
10. Apply Reducer.TrunsSideTransform.Inverted() to Reducer  and Terminal 3 connection

![image](images\Sample10.PNG)
 





