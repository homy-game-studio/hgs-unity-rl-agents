# [1.1.0](https://github.com/homy-game-studio/hgs-unity-rl-agents/compare/v1.0.0...v1.1.0) (2026-10-08)


### Bug Fixes

* adjust GA logic ([e66be25](https://github.com/homy-game-studio/hgs-unity-rl-agents/commit/e66be25500a42cd22ece8c88206be588f7e85d9c))
* correct bounds checking in Genome.GetGene ([c82a497](https://github.com/homy-game-studio/hgs-unity-rl-agents/commit/c82a4973e919924ad843447378b7af303a715816))
* driver model params ([41b81a4](https://github.com/homy-game-studio/hgs-unity-rl-agents/commit/41b81a44a96b2715572ae76e7e193725bf8f2ab9))
* improve null safety and avoid self-reference in CollisionSensor ([216e31f](https://github.com/homy-game-studio/hgs-unity-rl-agents/commit/216e31f2bf252e9b9ca19d5903378d3719f75b6e))
* improve simulation timing and add curriculum difficulty support ([cdfc6ec](https://github.com/homy-game-studio/hgs-unity-rl-agents/commit/cdfc6ec9f906255b46e82e4b2c7db5dbb3e175b9))
* ray sensor tag detection ([61411df](https://github.com/homy-game-studio/hgs-unity-rl-agents/commit/61411df74d2d3a6b1d5ad176f4f55370f4262128))
* remove random mutation values ([ad8a8d3](https://github.com/homy-game-studio/hgs-unity-rl-agents/commit/ad8a8d333739da694bdfc5de9b9860070118bfc2))
* stacker policy and params ([3b320ff](https://github.com/homy-game-studio/hgs-unity-rl-agents/commit/3b320ff47f4046f01cc3228836d69df62b3a269c))
* Update samples to use new genetic algorithm ([74aa37f](https://github.com/homy-game-studio/hgs-unity-rl-agents/commit/74aa37fe808fa56b44aabd7e2c44c408c91aa13a))


### Features

* Add auto completion to agent env ([f807695](https://github.com/homy-game-studio/hgs-unity-rl-agents/commit/f80769593936a22738b129487f7b2bab48caf0c3))
* add bias ([ce7d251](https://github.com/homy-game-studio/hgs-unity-rl-agents/commit/ce7d2511de99d220adecbcd315df031a63295c44))
* add CollisionSensor ([334b988](https://github.com/homy-game-studio/hgs-unity-rl-agents/commit/334b9880cbdb683a3c6c290090e64c33057e79e4))
* add curriculum learning and seed genome support to training ([2c55ef3](https://github.com/homy-game-studio/hgs-unity-rl-agents/commit/2c55ef3eafebad7d962ee344b6ad8a17cbd1d9c1))
* Add DriverSample ([091b7ca](https://github.com/homy-game-studio/hgs-unity-rl-agents/commit/091b7ca83e72cfdd704642e43ad0a4d13884bbc6))
* add Elman recurrent layer ([f5c53db](https://github.com/homy-game-studio/hgs-unity-rl-agents/commit/f5c53dbf438cb46d3b7bc83ec668b84c1cefec87))
* add frame stack layer for temporal input stacking ([3a36ca7](https://github.com/homy-game-studio/hgs-unity-rl-agents/commit/3a36ca7098dfdbeb4f4e9952c49145bf5c2f919c))
* Add GeneticAlgorithm to control agent ([c570061](https://github.com/homy-game-studio/hgs-unity-rl-agents/commit/c570061d71714cb92a3bade05ddb50a3bd4d3baa))
* add GRU layer implementation ([f1a347c](https://github.com/homy-game-studio/hgs-unity-rl-agents/commit/f1a347c500f1744870fcf88810614d58b7947797))
* add imitation learning module ([dc863c0](https://github.com/homy-game-studio/hgs-unity-rl-agents/commit/dc863c09aeb4875034f5e71a3b8655a57385380d))
* add imitation training scene for truck sample ([5cbdbc0](https://github.com/homy-game-studio/hgs-unity-rl-agents/commit/5cbdbc03639b90104b165eaf443da3d5a627e412))
* add LSTM layer implementation ([852f7be](https://github.com/homy-game-studio/hgs-unity-rl-agents/commit/852f7be1b9c18a1a207ab7b197bbe04449f425b7))
* add support for multiple layer types in Model ([29de81f](https://github.com/homy-game-studio/hgs-unity-rl-agents/commit/29de81fb825cd045a645843c873a533ecaf991c1))
* **parking-truck:** update training environment, agent and model ([8051c7b](https://github.com/homy-game-studio/hgs-unity-rl-agents/commit/8051c7b4d883fe28505905101ea31eedf8c59a33))
* add mutation subtraction after time ([80d9698](https://github.com/homy-game-studio/hgs-unity-rl-agents/commit/80d9698fa76096123e6aac93c871813a00615511))
* add save/load methods for models ([9875fcc](https://github.com/homy-game-studio/hgs-unity-rl-agents/commit/9875fcc56b5f91e55baca987c5246b5ec520d8ac))
* Add truck sample and fix gradients ([e5d896e](https://github.com/homy-game-studio/hgs-unity-rl-agents/commit/e5d896efeab99e1adb666bd2f161575c1fd4a357))
* Refactor Academy and Env to work with multiple agents ([3ba2dc6](https://github.com/homy-game-studio/hgs-unity-rl-agents/commit/3ba2dc6ce946f9fc8811199b7c4b7dd8541beb36))
* Refactor learning method and add parking truck sample ([7288017](https://github.com/homy-game-studio/hgs-unity-rl-agents/commit/728801791ff6996e21cb64055b190edd20a999a1))

# 1.0.0 (2024-04-24)


### Bug Fixes

* Remove ephocs param in Agent ([3150a45](https://github.com/homy-game-studio/hgs-unity-rl-agents/commit/3150a453df818e63d5f24e20eec384b138d71e63))


### Features

* Add .vscode to gitignore ([7a97fce](https://github.com/homy-game-studio/hgs-unity-rl-agents/commit/7a97fce3e5734202b8ac31571459260939e0f083))
* Add base Agent ([215efab](https://github.com/homy-game-studio/hgs-unity-rl-agents/commit/215efabcd33b4180c21628825ed851461f5c0e7f))
* Add Brain's ScriptableObject ([08b6d28](https://github.com/homy-game-studio/hgs-unity-rl-agents/commit/08b6d28a43e6fba51c8198e7ae0b9b3a80686cd7))
* Add DeepQLearning implemetation ([2ba5158](https://github.com/homy-game-studio/hgs-unity-rl-agents/commit/2ba5158bd1d0194685659ede57fbe706a12102ff))
* Add FollowTargetSample ([80af10b](https://github.com/homy-game-studio/hgs-unity-rl-agents/commit/80af10b691556976ab06103cbd2e7ec8c99ead1a))
* Add Models ([98b92ed](https://github.com/homy-game-studio/hgs-unity-rl-agents/commit/98b92ed94af55961e9a08a9bae83adbd522c6c6a))
* Add Neural Network implementation ([13e9e43](https://github.com/homy-game-studio/hgs-unity-rl-agents/commit/13e9e4393cc7c94381e33c6be1b45b24ced21d59))
* Add project dependencies ([74b28b8](https://github.com/homy-game-studio/hgs-unity-rl-agents/commit/74b28b87833705b933c3d28800924ef3c8951454))
* Add TrainManager ([f6f6b3b](https://github.com/homy-game-studio/hgs-unity-rl-agents/commit/f6f6b3b872907c50e4f22c826698ac7f48dbde8b))
* Add unity package files ([eec73db](https://github.com/homy-game-studio/hgs-unity-rl-agents/commit/eec73dbae867e5f8e83ca4674606da6d1103a00a))
