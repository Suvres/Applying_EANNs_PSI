#!/usr/bin/python3
"""
    Skrypt do trenowania sieci neuronowej z aplikacją do kontroli pojazdów
"""
import sys
from typing import List

import numpy as np
import torch
from torch import nn
import json


def transpose(A):
    M = len(A)
    N = len(A[0])
    B = [[0 for x in range(M)] for y in range(N)]
    for i in range(N):
        for j in range(M):
            B[i][j] = A[j][i]

    return B


def transposeOneDim(A, reverse: bool = False):
    M = 1
    N = len(A)
    if reverse:
        M, N = N, M

    B = [[0 for x in range(M)] for y in range(N)]
    for i in range(N):
        for j in range(M):
            if reverse:
                B[j] = A[j][i]
            else:
                B[i][j] = A[j]

    return B


class net(nn.Module):
    def __init__(self, inputs: List, outputs):
        super(net, self).__init__()
        self.fc = []
        self.sig = nn.Sigmoid()

        for i in range(len(inputs)):
            i_size = inputs[i]
            o_size = inputs[i + 1] - 1 if i < len(inputs) - 1 else outputs
            
            self.fc = [*self.fc, nn.Linear(i_size, o_size, bias=False)]

    def calc(self, weights: List, inputs):
        output = inputs
        for i in range(len(weights)):
            output = [*output, 1.0]
            output = output
            layer = self.fc[i]
            with torch.no_grad():
                layer.weight = nn.Parameter(torch.Tensor(transpose(weights[i])))

            output = layer(torch.Tensor(output))
            if i < len(weights) - 1:
                output = self.sig(output)

        return output


outputs = int(sys.argv[2])
file = sys.argv[1]

input_file = open(file)
weights = json.load(input_file)

weightsList = [len(w) for w in weights]
n = net(weightsList, outputs)

while True:
    com = input()
    args = np.array(com.split(' '))
    if args[0] == '0':
        exit(0)
    
    args = args[1:].astype(np.float64)
   
    outputs = n.calc(weights=weights, inputs=args)
    
    sys.stdout.write('{0:f}'.format(outputs[0]))
    for i in range(1, len(outputs)):
        sys.stdout.write(' {0:f}'.format(outputs[i]))
    
    sys.stdout.write('\n')
    sys.stdout.flush()
