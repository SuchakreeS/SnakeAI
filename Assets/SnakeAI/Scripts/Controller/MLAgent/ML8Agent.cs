﻿using Unity.MLAgents.Sensors;
using System;
using SnakeAI.Scripts.Entities;
using System.Collections.Generic;
using System.Linq;
using SnakeAI.Game;
using SnakeAI.Extention;
using UniRx;
using UnityEngine;
using Random = System.Random;

// ReSharper disable once CheckNamespace
namespace SnakeAI.Controller
{
	// ReSharper disable once InconsistentNaming
	public class ML8Agent : SnakeAgent
	{
		[SerializeField] private Vector2 m_WidthScope;
		[SerializeField] private Vector2 m_HeightScope;
        protected override void AddState(VectorSensor _sensor)
        {
	        // Add State
            _sensor.AddObservation(m_Controller.State.Head);
            _sensor.AddObservation(m_Controller.State.Food);
            _sensor.AddObservation(m_Controller.State.Tails.Count);
            
            // Food and Non-Food Direction Sets
			TraditionalObservable(_sensor);
			
			// Board Size
			_sensor.AddObservation(m_Controller.BoardSize);
        }
		private void TraditionalObservable(VectorSensor _sensor)
		{
			var state = m_Controller.State;
			// Need value function
            int NeedValue(int _value)
            {
                if (_value < 0) return -1;
                else if (_value > 0) return 1;
                else return 0;
            }
            // Head and Food position
            var head = state.Head;
            var food = state.Food;
            var horizontal = NeedValue((int) (food.x - head.x));
            var vertical = NeedValue((int) (food.y - head.y));
			var vectorDirection = new Vector2(horizontal, vertical);

            // Direction list lead to food
            var foodDirections = vectorDirection.GetDirectionType().ToList();
            foodDirections.Remove(State.GetNegativeDirection(m_Controller.Direction));
            
            // Direction list not lead to food
            var noFoodDirections = DirectionTypeExtension.AllDirections().Except(foodDirections).ToList();
            noFoodDirections.Remove(State.GetNegativeDirection(m_Controller.Direction));
			
            _sensor.AddObservation(vectorDirection);

			// Food Info
            _sensor.AddObservation(foodDirections.Contains(DirectionType.Up));
            _sensor.AddObservation(foodDirections.Contains(DirectionType.Down));
            _sensor.AddObservation(foodDirections.Contains(DirectionType.Right));
            _sensor.AddObservation(foodDirections.Contains(DirectionType.Left));

			// Non-Food Info
            _sensor.AddObservation(noFoodDirections.Contains(DirectionType.Up));
            _sensor.AddObservation(noFoodDirections.Contains(DirectionType.Down));
            _sensor.AddObservation(noFoodDirections.Contains(DirectionType.Right));
            _sensor.AddObservation(noFoodDirections.Contains(DirectionType.Left));
		}

		protected override void GameOverAction()
		{
			SetReward(-1f);
			OnWin();
		}

		protected override void WinAction()
		{
			SetReward(10f);
			OnWin();
		}

		protected override void EatAction() => SetReward(1f);

        private void OnWin()
        {
	        var width = UnityEngine.Random.Range(m_WidthScope.x, m_WidthScope.y);
	        var height = UnityEngine.Random.Range(m_HeightScope.x, m_HeightScope.y);
	        
	        m_Controller.SetBoardSize(new Vector2(width, height));
        }
    }
}