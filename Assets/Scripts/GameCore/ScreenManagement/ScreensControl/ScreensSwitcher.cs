using System;
using System.Collections.Generic;
using GameCore.ScreenManagement.ScreenCollection;
using GameCore.ScreenManagement.ScreenTransition.ScreenBlock;
using GameCore.Services.ServiceStructure;
using Zenject;

namespace GameCore.ScreenManagement.ScreensControl
{
	public class ScreensSwitcher : InstantMonoBehaviourService
	{
		private readonly Queue<ScreenSwitchCommand> _screenSwitchCommands = new();

		private ScreenBlocker _screenBlocker;

		private bool _isSwitching;
		private int _completionIterations;

		private bool _initialized;

		[Inject]
		private void Construct(ScreenBlocker screenBlocker)
		{
			_screenBlocker = screenBlocker;
		}

		public void SwitchScreens(UIScreen fromScreen, UIScreen toScreen,
			Action onCompleteTo = null, Action onCompleteFrom = null,
			ScreensTransitionType transitionType = ScreensTransitionType.InSeries, bool useScreenBlocker = true)
		{
			ScreenSwitchCommand screenSwitchCommand = new ScreenSwitchCommand(fromScreen, toScreen,
				onCompleteFrom, onCompleteTo,
				transitionType, useScreenBlocker);

			if (_isSwitching)
			{
				_screenSwitchCommands.Enqueue(screenSwitchCommand);
			}
			else
			{
				CompleteSwitchCommand(screenSwitchCommand);
			}
		}

		private void CompleteSwitchCommand(ScreenSwitchCommand screenSwitchCommand)
		{
			_isSwitching = true;
			
			Action iterateAction = () => ++_completionIterations;
			Func<bool> completeTransitionChecker = () => _completionIterations >= 2;
			
			Action onCompleteFrom = () =>
			{
				screenSwitchCommand.OnCompleteFromAction?.Invoke();
				HandleCompletedScreenAnimation(iterateAction, completeTransitionChecker, screenSwitchCommand);
			};

			Action onCompleteTo = () =>
			{
				screenSwitchCommand.OnCompleteToAction?.Invoke();
				HandleCompletedScreenAnimation(iterateAction, completeTransitionChecker, screenSwitchCommand);
			};

			if (screenSwitchCommand.UseBlocker)
			{
				_screenBlocker.ChangeBlockScreenState(true);
			}

			if (screenSwitchCommand.TransitionType == ScreensTransitionType.Parallel)
			{
				Action completeAction = GenerateParallelTransition(screenSwitchCommand.FromScreen,
					screenSwitchCommand.ToScreen, onCompleteFrom, onCompleteTo);
				
				completeAction.Invoke();
			}
			else
			{
				Action completeAction = GenerateInSeriesTransition(screenSwitchCommand.FromScreen,
					screenSwitchCommand.ToScreen, onCompleteFrom, onCompleteTo);
				
				completeAction.Invoke();
			}
		}

		private void HandleCompletedScreenAnimation(Action completeIterationAction,
			Func<bool> onCompleteScreenTransition, ScreenSwitchCommand attachedCommand)
		{
			completeIterationAction?.Invoke();

			if (onCompleteScreenTransition != null && !onCompleteScreenTransition.Invoke())
				return;

			_isSwitching = false;
			_completionIterations = 0;

			if (attachedCommand.UseBlocker)
			{
				_screenBlocker.ChangeBlockScreenState(false);
			}
			
			if (_screenSwitchCommands.Count > 0)
			{
				ScreenSwitchCommand screenSwitchCommand = _screenSwitchCommands.Dequeue();
				CompleteSwitchCommand(screenSwitchCommand);
			}
		}

		private Action GenerateParallelTransition(UIScreen fromScreen, UIScreen toScreen,
			Action onCompleteFrom, Action onCompleteTo)
		{
			Action generatedTransition = () =>
			{
				if (fromScreen)
				{
					fromScreen.Stop(true);
					fromScreen.Hide(onCompleteFrom);
				}
				else
				{
					onCompleteFrom?.Invoke();
				}

				if (toScreen)
				{
					toScreen.Stop(true);
					toScreen.Show(onCompleteTo);
				}
				else
				{
					onCompleteTo?.Invoke();
				}
			};

			return generatedTransition;
		}
		
		private Action GenerateInSeriesTransition(UIScreen fromScreen, UIScreen toScreen,
			Action onCompleteFrom, Action onCompleteTo)
		{
			Action handleToScreen = () =>
			{
				if (toScreen)
				{
					toScreen.Stop(true);
					toScreen.Show(onCompleteTo);
				}
				else
				{
					onCompleteTo?.Invoke();
				}
			};
			
			Action generatedTransition = () =>
			{
				if (fromScreen)
				{
					fromScreen.Stop(true);
					fromScreen.Hide(() =>
					{
						onCompleteFrom?.Invoke();
						handleToScreen();
					});
				}
				else
				{
					onCompleteFrom?.Invoke();
					handleToScreen();
				}
			};

			return generatedTransition;
		}

		private void ToDefaultState()
		{
			_screenSwitchCommands.Clear();
		}

		private void OnDestroy()
		{
			ToDefaultState();
		}
	}
}