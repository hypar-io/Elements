using System;
using System.Collections.Generic;
using Elements.Algorithms;
using Elements.Tests;
using System.Linq;
using Xunit;

namespace Elements.Algorithms.Tests
{
    public class AlgorithmsTests : ModelTest
    {
        public AlgorithmsTests()
        {
            this.GenerateIfc = false;
        }

        [Fact]
        public void DsuInitTest()
        {
            var dsu = new DisjointSetUnion(0);
            dsu = new DisjointSetUnion(1000);
        }

        [Fact]
        public void DsuTest()
        {
            var dsu = new DisjointSetUnion(10);
            bool res;
            res = dsu.AddEdge(0, 1); Assert.True(res);
            res = dsu.AddEdge(9, 3); Assert.True(res);
            res = dsu.AddEdge(9, 6); Assert.True(res);
            res = dsu.AddEdge(6, 0); Assert.True(res);
            res = dsu.AddEdge(6, 2); Assert.True(res);
            res = dsu.AddEdge(5, 3); Assert.True(res);
            res = dsu.AddEdge(1, 3); Assert.True(!res);
            res = dsu.AddEdge(9, 0); Assert.True(!res);
            res = dsu.AddEdge(8, 7); Assert.True(res);
            res = dsu.AddEdge(5, 6); Assert.True(!res);
            res = dsu.AddEdge(9, 2); Assert.True(!res);

            Assert.Equal(3, dsu.NumComponents);
            Assert.Equal(dsu.GetParent(0), dsu.GetParent(1));
            Assert.Equal(dsu.GetParent(0), dsu.GetParent(2));
            Assert.Equal(dsu.GetParent(0), dsu.GetParent(3));
            Assert.Equal(dsu.GetParent(0), dsu.GetParent(5));
            Assert.Equal(dsu.GetParent(0), dsu.GetParent(6));
            Assert.Equal(dsu.GetParent(0), dsu.GetParent(9));
            Assert.Equal(dsu.GetParent(7), dsu.GetParent(8));
            Assert.NotEqual(dsu.GetParent(0), dsu.GetParent(4));
            Assert.NotEqual(dsu.GetParent(0), dsu.GetParent(8));
            Assert.NotEqual(dsu.GetParent(8), dsu.GetParent(4));
            Assert.Equal(7, dsu.ComponentSize(0));
            Assert.Equal(2, dsu.ComponentSize(7));
            Assert.Equal(1, dsu.ComponentSize(4));
        }

        [Fact]
        public void BinaryHeapTest()
        {
            var heap = new BinaryHeap<double, int>();
            (double, int) tmp;
            // the following code was used to generate the test
            /*
            import random as rnd
            rnd.seed(123)

            m = 300
            n0 = 40
            a = []
            for _ in range(m):
                n = len(a)
                q1 = n0 / (n0 + n)
    
                r = rnd.random()
    
                if r < q1:
                    k = rnd.random()
                    v = rnd.randint(-10000, 10000)
                    a.append((k,v))
                    a = sorted(a)
                    print("heap.Insert({}, {});".format(k, v))
                else:
                    k, v = a[-1]
                    a = a[:-1]
                    print("tmp = heap.Extract(); Assert.Equal({}, tmp.Item1, 12); Assert.Equal({}, tmp.Item2);".format(k, v))
            */
            heap.Insert(0.08718667752263232, 3344);
            heap.Insert(0.8385035164743577, -8750);
            heap.Insert(0.5623187149479814, 1167);
            heap.Insert(0.1596623967219699, 1049);
            heap.Insert(0.7016897890356433, -4634);
            heap.Insert(0.4362757934152184, -7130);
            tmp = heap.Extract(); Assert.Equal(0.8385035164743577, tmp.Item1, 12); Assert.Equal(-8750, tmp.Item2);
            heap.Insert(0.00659504022791213, 4690);
            heap.Insert(0.043887451982234094, -5342);
            heap.Insert(0.9066079782041607, -439);
            tmp = heap.Extract(); Assert.Equal(0.9066079782041607, tmp.Item1, 12); Assert.Equal(-439, tmp.Item2);
            heap.Insert(0.2653217052615633, -8796);
            heap.Insert(0.3434621581559646, 5825);
            heap.Insert(0.6089025261905533, 7271);
            heap.Insert(0.9605634981040891, -9589);
            heap.Insert(0.7692790899139238, 6801);
            heap.Insert(0.5385194319363926, 9546);
            tmp = heap.Extract(); Assert.Equal(0.9605634981040891, tmp.Item1, 12); Assert.Equal(-9589, tmp.Item2);
            heap.Insert(0.6684702219031471, 2272);
            heap.Insert(0.847341781513193, -4054);
            heap.Insert(0.08260976458822122, 5954);
            heap.Insert(0.17012688767090822, 1040);
            heap.Insert(0.8839165946341363, 4967);
            heap.Insert(0.778738568247155, -9466);
            heap.Insert(0.08815729492607605, 1911);
            heap.Insert(0.35191368309820115, -2492);
            heap.Insert(0.5010076293622207, -8043);
            heap.Insert(0.9917930997519157, -2353);
            heap.Insert(0.9806745885132978, -4962);
            tmp = heap.Extract(); Assert.Equal(0.9917930997519157, tmp.Item1, 12); Assert.Equal(-2353, tmp.Item2);
            heap.Insert(0.7516501580275301, -8554);
            heap.Insert(0.45634032184762485, -9312);
            heap.Insert(0.3520475304496762, -1090);
            tmp = heap.Extract(); Assert.Equal(0.9806745885132978, tmp.Item1, 12); Assert.Equal(-4962, tmp.Item2);
            tmp = heap.Extract(); Assert.Equal(0.8839165946341363, tmp.Item1, 12); Assert.Equal(4967, tmp.Item2);
            heap.Insert(0.6175846313134489, 6722);
            heap.Insert(0.08878890486736224, 4195);
            tmp = heap.Extract(); Assert.Equal(0.847341781513193, tmp.Item1, 12); Assert.Equal(-4054, tmp.Item2);
            tmp = heap.Extract(); Assert.Equal(0.778738568247155, tmp.Item1, 12); Assert.Equal(-9466, tmp.Item2);
            heap.Insert(0.5465829051599347, 3245);
            heap.Insert(0.3224579636941851, 5483);
            heap.Insert(0.5615882153423916, 3568);
            heap.Insert(0.6391279372281128, -4631);
            tmp = heap.Extract(); Assert.Equal(0.7692790899139238, tmp.Item1, 12); Assert.Equal(6801, tmp.Item2);
            heap.Insert(0.7177179073521607, -5282);
            heap.Insert(0.23496343749846516, -7432);
            tmp = heap.Extract(); Assert.Equal(0.7516501580275301, tmp.Item1, 12); Assert.Equal(-8554, tmp.Item2);
            tmp = heap.Extract(); Assert.Equal(0.7177179073521607, tmp.Item1, 12); Assert.Equal(-5282, tmp.Item2);
            tmp = heap.Extract(); Assert.Equal(0.7016897890356433, tmp.Item1, 12); Assert.Equal(-4634, tmp.Item2);
            heap.Insert(0.3061637591282965, -459);
            tmp = heap.Extract(); Assert.Equal(0.6684702219031471, tmp.Item1, 12); Assert.Equal(2272, tmp.Item2);
            heap.Insert(0.1264699588337581, 1066);
            heap.Insert(0.6296176252275832, 9310);
            heap.Insert(0.06205576465547391, -5544);
            heap.Insert(0.020612354200629013, 4736);
            tmp = heap.Extract(); Assert.Equal(0.6391279372281128, tmp.Item1, 12); Assert.Equal(-4631, tmp.Item2);
            tmp = heap.Extract(); Assert.Equal(0.6296176252275832, tmp.Item1, 12); Assert.Equal(9310, tmp.Item2);
            tmp = heap.Extract(); Assert.Equal(0.6175846313134489, tmp.Item1, 12); Assert.Equal(6722, tmp.Item2);
            tmp = heap.Extract(); Assert.Equal(0.6089025261905533, tmp.Item1, 12); Assert.Equal(7271, tmp.Item2);
            heap.Insert(0.7911091579073943, -9075);
            heap.Insert(0.32485631757523614, 6935);
            heap.Insert(0.13737149792589898, 6388);
            heap.Insert(0.7263261628821532, -2631);
            tmp = heap.Extract(); Assert.Equal(0.7911091579073943, tmp.Item1, 12); Assert.Equal(-9075, tmp.Item2);
            heap.Insert(0.2717513883845203, 6590);
            heap.Insert(0.8864946761880608, -1332);
            heap.Insert(0.938262488697077, -7367);
            tmp = heap.Extract(); Assert.Equal(0.938262488697077, tmp.Item1, 12); Assert.Equal(-7367, tmp.Item2);
            heap.Insert(0.758115135097988, -9438);
            heap.Insert(0.5646244900742771, -7212);
            tmp = heap.Extract(); Assert.Equal(0.8864946761880608, tmp.Item1, 12); Assert.Equal(-1332, tmp.Item2);
            heap.Insert(0.5545347074991613, -5627);
            tmp = heap.Extract(); Assert.Equal(0.758115135097988, tmp.Item1, 12); Assert.Equal(-9438, tmp.Item2);
            tmp = heap.Extract(); Assert.Equal(0.7263261628821532, tmp.Item1, 12); Assert.Equal(-2631, tmp.Item2);
            heap.Insert(0.7860800542078382, 5858);
            heap.Insert(0.3983561230969015, -9076);
            heap.Insert(0.8327770443873531, 8938);
            tmp = heap.Extract(); Assert.Equal(0.8327770443873531, tmp.Item1, 12); Assert.Equal(8938, tmp.Item2);
            heap.Insert(0.914498271227999, 6425);
            tmp = heap.Extract(); Assert.Equal(0.914498271227999, tmp.Item1, 12); Assert.Equal(6425, tmp.Item2);
            tmp = heap.Extract(); Assert.Equal(0.7860800542078382, tmp.Item1, 12); Assert.Equal(5858, tmp.Item2);
            heap.Insert(0.7937888040432782, 9700);
            tmp = heap.Extract(); Assert.Equal(0.7937888040432782, tmp.Item1, 12); Assert.Equal(9700, tmp.Item2);
            heap.Insert(0.6957821740554558, -5992);
            tmp = heap.Extract(); Assert.Equal(0.6957821740554558, tmp.Item1, 12); Assert.Equal(-5992, tmp.Item2);
            heap.Insert(0.4607614901799043, 3787);
            heap.Insert(0.21659433670555028, 4714);
            heap.Insert(0.7537139593993878, -4080);
            heap.Insert(0.8060883139255806, 3589);
            heap.Insert(0.2654792281192372, -4270);
            tmp = heap.Extract(); Assert.Equal(0.8060883139255806, tmp.Item1, 12); Assert.Equal(3589, tmp.Item2);
            heap.Insert(0.0691592314964744, -304);
            heap.Insert(0.2654160671146927, 9839);
            heap.Insert(0.9442210523827754, -8310);
            tmp = heap.Extract(); Assert.Equal(0.9442210523827754, tmp.Item1, 12); Assert.Equal(-8310, tmp.Item2);
            heap.Insert(0.46554810981102523, 639);
            heap.Insert(0.4454228972339206, 4392);
            tmp = heap.Extract(); Assert.Equal(0.7537139593993878, tmp.Item1, 12); Assert.Equal(-4080, tmp.Item2);
            heap.Insert(0.9487882903998216, 4925);
            heap.Insert(0.9054631957637249, -989);
            heap.Insert(0.04876860366915037, -5959);
            heap.Insert(0.6602046601210401, -5679);
            heap.Insert(0.04276007019212802, 7757);
            heap.Insert(0.8704672557844291, -2763);
            tmp = heap.Extract(); Assert.Equal(0.9487882903998216, tmp.Item1, 12); Assert.Equal(4925, tmp.Item2);
            heap.Insert(0.504116258767117, 9706);
            tmp = heap.Extract(); Assert.Equal(0.9054631957637249, tmp.Item1, 12); Assert.Equal(-989, tmp.Item2);
            heap.Insert(0.5801048271426202, -9544);
            heap.Insert(0.3865008743671855, -7948);
            tmp = heap.Extract(); Assert.Equal(0.8704672557844291, tmp.Item1, 12); Assert.Equal(-2763, tmp.Item2);
            heap.Insert(0.9954301447872572, 1718);
            heap.Insert(0.6070585475338274, 6279);
            tmp = heap.Extract(); Assert.Equal(0.9954301447872572, tmp.Item1, 12); Assert.Equal(1718, tmp.Item2);
            heap.Insert(0.2694497892616108, -4563);
            tmp = heap.Extract(); Assert.Equal(0.6602046601210401, tmp.Item1, 12); Assert.Equal(-5679, tmp.Item2);
            tmp = heap.Extract(); Assert.Equal(0.6070585475338274, tmp.Item1, 12); Assert.Equal(6279, tmp.Item2);
            tmp = heap.Extract(); Assert.Equal(0.5801048271426202, tmp.Item1, 12); Assert.Equal(-9544, tmp.Item2);
            heap.Insert(0.8656732430775638, 5315);
            heap.Insert(0.5278104401540661, -6618);
            heap.Insert(0.5449613597795614, -848);
            tmp = heap.Extract(); Assert.Equal(0.8656732430775638, tmp.Item1, 12); Assert.Equal(5315, tmp.Item2);
            tmp = heap.Extract(); Assert.Equal(0.5646244900742771, tmp.Item1, 12); Assert.Equal(-7212, tmp.Item2);
            tmp = heap.Extract(); Assert.Equal(0.5623187149479814, tmp.Item1, 12); Assert.Equal(1167, tmp.Item2);
            tmp = heap.Extract(); Assert.Equal(0.5615882153423916, tmp.Item1, 12); Assert.Equal(3568, tmp.Item2);
            heap.Insert(0.7977685167753531, 4698);
            heap.Insert(0.5700076403666973, 4287);
            tmp = heap.Extract(); Assert.Equal(0.7977685167753531, tmp.Item1, 12); Assert.Equal(4698, tmp.Item2);
            heap.Insert(0.13456051196500585, -3066);
            tmp = heap.Extract(); Assert.Equal(0.5700076403666973, tmp.Item1, 12); Assert.Equal(4287, tmp.Item2);
            tmp = heap.Extract(); Assert.Equal(0.5545347074991613, tmp.Item1, 12); Assert.Equal(-5627, tmp.Item2);
            heap.Insert(0.20817480291008883, 3226);
            tmp = heap.Extract(); Assert.Equal(0.5465829051599347, tmp.Item1, 12); Assert.Equal(3245, tmp.Item2);
            heap.Insert(0.6562134498500771, 7921);
            heap.Insert(0.9246721337844935, -1922);
            tmp = heap.Extract(); Assert.Equal(0.9246721337844935, tmp.Item1, 12); Assert.Equal(-1922, tmp.Item2);
            tmp = heap.Extract(); Assert.Equal(0.6562134498500771, tmp.Item1, 12); Assert.Equal(7921, tmp.Item2);
            heap.Insert(0.29138316904878636, -3908);
            heap.Insert(0.2825479780953366, 643);
            heap.Insert(0.02596629223721869, 1846);
            tmp = heap.Extract(); Assert.Equal(0.5449613597795614, tmp.Item1, 12); Assert.Equal(-848, tmp.Item2);
            heap.Insert(0.4142883511040336, 6729);
            tmp = heap.Extract(); Assert.Equal(0.5385194319363926, tmp.Item1, 12); Assert.Equal(9546, tmp.Item2);
            heap.Insert(0.5604854687225679, 2782);
            heap.Insert(0.9041338972518844, -8605);
            heap.Insert(0.47564560719744, -6500);
            heap.Insert(0.46010439043098517, 1563);
            tmp = heap.Extract(); Assert.Equal(0.9041338972518844, tmp.Item1, 12); Assert.Equal(-8605, tmp.Item2);
            heap.Insert(0.520872260279999, -539);
            heap.Insert(0.39058623415691895, -8453);
            heap.Insert(0.8297004840164622, -6892);
            tmp = heap.Extract(); Assert.Equal(0.8297004840164622, tmp.Item1, 12); Assert.Equal(-6892, tmp.Item2);
            heap.Insert(0.08277047261975956, -3252);
            tmp = heap.Extract(); Assert.Equal(0.5604854687225679, tmp.Item1, 12); Assert.Equal(2782, tmp.Item2);
            heap.Insert(0.12614697125801966, 5570);
            heap.Insert(0.8627734035649545, -4674);
            tmp = heap.Extract(); Assert.Equal(0.8627734035649545, tmp.Item1, 12); Assert.Equal(-4674, tmp.Item2);
            heap.Insert(0.03229110016086212, -5203);
            heap.Insert(0.4302053521347048, -3420);
            tmp = heap.Extract(); Assert.Equal(0.5278104401540661, tmp.Item1, 12); Assert.Equal(-6618, tmp.Item2);
            heap.Insert(0.15169746513952687, 5117);
            tmp = heap.Extract(); Assert.Equal(0.520872260279999, tmp.Item1, 12); Assert.Equal(-539, tmp.Item2);
            heap.Insert(0.041178475014365556, 4400);
            tmp = heap.Extract(); Assert.Equal(0.504116258767117, tmp.Item1, 12); Assert.Equal(9706, tmp.Item2);
            tmp = heap.Extract(); Assert.Equal(0.5010076293622207, tmp.Item1, 12); Assert.Equal(-8043, tmp.Item2);
            tmp = heap.Extract(); Assert.Equal(0.47564560719744, tmp.Item1, 12); Assert.Equal(-6500, tmp.Item2);
            heap.Insert(0.4313008512665134, -5314);
            heap.Insert(0.009541469005388659, 727);
            tmp = heap.Extract(); Assert.Equal(0.46554810981102523, tmp.Item1, 12); Assert.Equal(639, tmp.Item2);
            heap.Insert(0.2289026589293196, -8006);
            heap.Insert(0.06460294954723267, -1819);
            heap.Insert(0.1251144152681204, -9974);
            tmp = heap.Extract(); Assert.Equal(0.4607614901799043, tmp.Item1, 12); Assert.Equal(3787, tmp.Item2);
            tmp = heap.Extract(); Assert.Equal(0.46010439043098517, tmp.Item1, 12); Assert.Equal(1563, tmp.Item2);
            tmp = heap.Extract(); Assert.Equal(0.45634032184762485, tmp.Item1, 12); Assert.Equal(-9312, tmp.Item2);
            tmp = heap.Extract(); Assert.Equal(0.4454228972339206, tmp.Item1, 12); Assert.Equal(4392, tmp.Item2);
            tmp = heap.Extract(); Assert.Equal(0.4362757934152184, tmp.Item1, 12); Assert.Equal(-7130, tmp.Item2);
            heap.Insert(0.4208720324673456, 8192);
            heap.Insert(0.626088627653204, 3659);
            tmp = heap.Extract(); Assert.Equal(0.626088627653204, tmp.Item1, 12); Assert.Equal(3659, tmp.Item2);
            tmp = heap.Extract(); Assert.Equal(0.4313008512665134, tmp.Item1, 12); Assert.Equal(-5314, tmp.Item2);
            heap.Insert(0.6902319553617339, -9893);
            tmp = heap.Extract(); Assert.Equal(0.6902319553617339, tmp.Item1, 12); Assert.Equal(-9893, tmp.Item2);
            tmp = heap.Extract(); Assert.Equal(0.4302053521347048, tmp.Item1, 12); Assert.Equal(-3420, tmp.Item2);
            heap.Insert(0.2941349378219613, 2403);
            tmp = heap.Extract(); Assert.Equal(0.4208720324673456, tmp.Item1, 12); Assert.Equal(8192, tmp.Item2);
            tmp = heap.Extract(); Assert.Equal(0.4142883511040336, tmp.Item1, 12); Assert.Equal(6729, tmp.Item2);
            heap.Insert(0.104618212084711, 9540);
            tmp = heap.Extract(); Assert.Equal(0.3983561230969015, tmp.Item1, 12); Assert.Equal(-9076, tmp.Item2);
            tmp = heap.Extract(); Assert.Equal(0.39058623415691895, tmp.Item1, 12); Assert.Equal(-8453, tmp.Item2);
            heap.Insert(0.6783792530790603, -3720);
            heap.Insert(0.3582671689046272, 5757);
            heap.Insert(0.8903160955967432, 7847);
            heap.Insert(0.02671482607993625, -8981);
            tmp = heap.Extract(); Assert.Equal(0.8903160955967432, tmp.Item1, 12); Assert.Equal(7847, tmp.Item2);
            tmp = heap.Extract(); Assert.Equal(0.6783792530790603, tmp.Item1, 12); Assert.Equal(-3720, tmp.Item2);
            tmp = heap.Extract(); Assert.Equal(0.3865008743671855, tmp.Item1, 12); Assert.Equal(-7948, tmp.Item2);
            tmp = heap.Extract(); Assert.Equal(0.3582671689046272, tmp.Item1, 12); Assert.Equal(5757, tmp.Item2);
            heap.Insert(0.10081332278536825, 9814);
            heap.Insert(0.12125562796565204, -7589);
            heap.Insert(0.9146665279183616, 1834);
            heap.Insert(0.4107476777789437, -1481);
            tmp = heap.Extract(); Assert.Equal(0.9146665279183616, tmp.Item1, 12); Assert.Equal(1834, tmp.Item2);
            tmp = heap.Extract(); Assert.Equal(0.4107476777789437, tmp.Item1, 12); Assert.Equal(-1481, tmp.Item2);
            heap.Insert(0.5349347004365425, 806);
            heap.Insert(0.8010121482460647, 106);
            heap.Insert(0.23497705910616173, -6934);
            tmp = heap.Extract(); Assert.Equal(0.8010121482460647, tmp.Item1, 12); Assert.Equal(106, tmp.Item2);
            tmp = heap.Extract(); Assert.Equal(0.5349347004365425, tmp.Item1, 12); Assert.Equal(806, tmp.Item2);
            tmp = heap.Extract(); Assert.Equal(0.3520475304496762, tmp.Item1, 12); Assert.Equal(-1090, tmp.Item2);
            heap.Insert(0.3882573829413716, 1756);
            tmp = heap.Extract(); Assert.Equal(0.3882573829413716, tmp.Item1, 12); Assert.Equal(1756, tmp.Item2);
            heap.Insert(0.9044058407269734, 2694);
            heap.Insert(0.20965584546314464, -9860);
            tmp = heap.Extract(); Assert.Equal(0.9044058407269734, tmp.Item1, 12); Assert.Equal(2694, tmp.Item2);
            heap.Insert(0.8471740639497004, -4278);
            tmp = heap.Extract(); Assert.Equal(0.8471740639497004, tmp.Item1, 12); Assert.Equal(-4278, tmp.Item2);
            heap.Insert(0.7773922226126466, 2783);
            tmp = heap.Extract(); Assert.Equal(0.7773922226126466, tmp.Item1, 12); Assert.Equal(2783, tmp.Item2);
            heap.Insert(0.023827367439984704, 8643);
            heap.Insert(0.5684227872905153, 7812);
            tmp = heap.Extract(); Assert.Equal(0.5684227872905153, tmp.Item1, 12); Assert.Equal(7812, tmp.Item2);
            tmp = heap.Extract(); Assert.Equal(0.35191368309820115, tmp.Item1, 12); Assert.Equal(-2492, tmp.Item2);
            heap.Insert(0.5474033265135162, 1082);
            tmp = heap.Extract(); Assert.Equal(0.5474033265135162, tmp.Item1, 12); Assert.Equal(1082, tmp.Item2);
            tmp = heap.Extract(); Assert.Equal(0.3434621581559646, tmp.Item1, 12); Assert.Equal(5825, tmp.Item2);
            heap.Insert(0.8802231100289032, -3448);
            heap.Insert(0.49749399755125334, 3851);
            heap.Insert(0.799157893662416, -4956);
            heap.Insert(0.04734306447118275, -2714);
            tmp = heap.Extract(); Assert.Equal(0.8802231100289032, tmp.Item1, 12); Assert.Equal(-3448, tmp.Item2);
            heap.Insert(0.5782904398820602, -3780);
            heap.Insert(0.7183407531468159, 4222);
            tmp = heap.Extract(); Assert.Equal(0.799157893662416, tmp.Item1, 12); Assert.Equal(-4956, tmp.Item2);
            tmp = heap.Extract(); Assert.Equal(0.7183407531468159, tmp.Item1, 12); Assert.Equal(4222, tmp.Item2);
            heap.Insert(0.8309336368919169, -2916);
            heap.Insert(0.042731731675003104, 7528);
            tmp = heap.Extract(); Assert.Equal(0.8309336368919169, tmp.Item1, 12); Assert.Equal(-2916, tmp.Item2);
            tmp = heap.Extract(); Assert.Equal(0.5782904398820602, tmp.Item1, 12); Assert.Equal(-3780, tmp.Item2);
            heap.Insert(0.03563582268495313, 7213);
            heap.Insert(0.7139256153312372, 3493);
            tmp = heap.Extract(); Assert.Equal(0.7139256153312372, tmp.Item1, 12); Assert.Equal(3493, tmp.Item2);
            tmp = heap.Extract(); Assert.Equal(0.49749399755125334, tmp.Item1, 12); Assert.Equal(3851, tmp.Item2);
            tmp = heap.Extract(); Assert.Equal(0.32485631757523614, tmp.Item1, 12); Assert.Equal(6935, tmp.Item2);
            heap.Insert(0.18244040491373792, 3261);
            tmp = heap.Extract(); Assert.Equal(0.3224579636941851, tmp.Item1, 12); Assert.Equal(5483, tmp.Item2);
            tmp = heap.Extract(); Assert.Equal(0.3061637591282965, tmp.Item1, 12); Assert.Equal(-459, tmp.Item2);
            heap.Insert(0.043505208313409205, -5012);
            tmp = heap.Extract(); Assert.Equal(0.2941349378219613, tmp.Item1, 12); Assert.Equal(2403, tmp.Item2);
            tmp = heap.Extract(); Assert.Equal(0.29138316904878636, tmp.Item1, 12); Assert.Equal(-3908, tmp.Item2);
            tmp = heap.Extract(); Assert.Equal(0.2825479780953366, tmp.Item1, 12); Assert.Equal(643, tmp.Item2);
            tmp = heap.Extract(); Assert.Equal(0.2717513883845203, tmp.Item1, 12); Assert.Equal(6590, tmp.Item2);
            heap.Insert(0.510106648659718, -1376);
            heap.Insert(0.8445879458191141, 7482);
            tmp = heap.Extract(); Assert.Equal(0.8445879458191141, tmp.Item1, 12); Assert.Equal(7482, tmp.Item2);
            tmp = heap.Extract(); Assert.Equal(0.510106648659718, tmp.Item1, 12); Assert.Equal(-1376, tmp.Item2);
            tmp = heap.Extract(); Assert.Equal(0.2694497892616108, tmp.Item1, 12); Assert.Equal(-4563, tmp.Item2);
            tmp = heap.Extract(); Assert.Equal(0.2654792281192372, tmp.Item1, 12); Assert.Equal(-4270, tmp.Item2);
            tmp = heap.Extract(); Assert.Equal(0.2654160671146927, tmp.Item1, 12); Assert.Equal(9839, tmp.Item2);
            tmp = heap.Extract(); Assert.Equal(0.2653217052615633, tmp.Item1, 12); Assert.Equal(-8796, tmp.Item2);
            tmp = heap.Extract(); Assert.Equal(0.23497705910616173, tmp.Item1, 12); Assert.Equal(-6934, tmp.Item2);
            heap.Insert(0.7588916167379602, 9681);
            heap.Insert(0.9103535835090739, -1930);
            tmp = heap.Extract(); Assert.Equal(0.9103535835090739, tmp.Item1, 12); Assert.Equal(-1930, tmp.Item2);
            tmp = heap.Extract(); Assert.Equal(0.7588916167379602, tmp.Item1, 12); Assert.Equal(9681, tmp.Item2);
            heap.Insert(0.3716636247654175, 5250);
            heap.Insert(0.7401388755754874, 7645);
            tmp = heap.Extract(); Assert.Equal(0.7401388755754874, tmp.Item1, 12); Assert.Equal(7645, tmp.Item2);
            tmp = heap.Extract(); Assert.Equal(0.3716636247654175, tmp.Item1, 12); Assert.Equal(5250, tmp.Item2);
            tmp = heap.Extract(); Assert.Equal(0.23496343749846516, tmp.Item1, 12); Assert.Equal(-7432, tmp.Item2);
            tmp = heap.Extract(); Assert.Equal(0.2289026589293196, tmp.Item1, 12); Assert.Equal(-8006, tmp.Item2);
            heap.Insert(0.03967715334068245, -8284);
            tmp = heap.Extract(); Assert.Equal(0.21659433670555028, tmp.Item1, 12); Assert.Equal(4714, tmp.Item2);
            heap.Insert(0.37245172824233197, 3807);
            tmp = heap.Extract(); Assert.Equal(0.37245172824233197, tmp.Item1, 12); Assert.Equal(3807, tmp.Item2);
            tmp = heap.Extract(); Assert.Equal(0.20965584546314464, tmp.Item1, 12); Assert.Equal(-9860, tmp.Item2);
            tmp = heap.Extract(); Assert.Equal(0.20817480291008883, tmp.Item1, 12); Assert.Equal(3226, tmp.Item2);
            tmp = heap.Extract(); Assert.Equal(0.18244040491373792, tmp.Item1, 12); Assert.Equal(3261, tmp.Item2);
            heap.Insert(0.38369455915168804, -5551);
            heap.Insert(0.7128153450744859, 5994);
            tmp = heap.Extract(); Assert.Equal(0.7128153450744859, tmp.Item1, 12); Assert.Equal(5994, tmp.Item2);
            tmp = heap.Extract(); Assert.Equal(0.38369455915168804, tmp.Item1, 12); Assert.Equal(-5551, tmp.Item2);
            tmp = heap.Extract(); Assert.Equal(0.17012688767090822, tmp.Item1, 12); Assert.Equal(1040, tmp.Item2);
            tmp = heap.Extract(); Assert.Equal(0.1596623967219699, tmp.Item1, 12); Assert.Equal(1049, tmp.Item2);
            heap.Insert(0.3153034443987687, 9815);
            tmp = heap.Extract(); Assert.Equal(0.3153034443987687, tmp.Item1, 12); Assert.Equal(9815, tmp.Item2);
            heap.Insert(0.9026380397235492, -1238);
            heap.Insert(0.48088838291507563, -2291);
            heap.Insert(0.7127604472684225, -8272);
            tmp = heap.Extract(); Assert.Equal(0.9026380397235492, tmp.Item1, 12); Assert.Equal(-1238, tmp.Item2);
            tmp = heap.Extract(); Assert.Equal(0.7127604472684225, tmp.Item1, 12); Assert.Equal(-8272, tmp.Item2);
            heap.Insert(0.9332943785229327, -5403);
            heap.Insert(0.6490472195091884, 3087);
            tmp = heap.Extract(); Assert.Equal(0.9332943785229327, tmp.Item1, 12); Assert.Equal(-5403, tmp.Item2);
            heap.Insert(0.5581625467596122, -1840);
            heap.Insert(0.6540263457793537, 5922);
            heap.Insert(0.005240965415516552, -4896);
            heap.Insert(0.9871504282705488, 6545);
            tmp = heap.Extract(); Assert.Equal(0.9871504282705488, tmp.Item1, 12); Assert.Equal(6545, tmp.Item2);
            heap.Insert(0.4268769304941178, 5478);
            heap.Insert(0.04728084532819088, 4069);
        }

        [Fact]
        public void SteinerTreeTest()
        {
            var graph = new SteinerTreeCalculator(6);
            graph.AddEdge(0, 1, 7);
            graph.AddEdge(2, 4, 3);
            graph.AddEdge(5, 3, 4);
            graph.AddEdge(1, 2, 1);
            graph.AddEdge(5, 4, 10);
            graph.AddEdge(2, 3, 1);
            graph.AddEdge(0, 5, 6);
            graph.AddEdge(4, 3, 1);
            graph.AddEdge(1, 5, 5);

            var edges = new List<(int, int, double)>(graph.GetTree(new int[4] {0, 2, 4, 5 }));
            Assert.Equal(4, edges.Count);
        }
    }
}
