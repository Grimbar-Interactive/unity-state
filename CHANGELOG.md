# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/)

==
## [1.3.6] - 2024-02-13
### Added
- Added scripting defines.

## [1.3.5] - 2024-02-01
### Added
- Added support for Unity 2019.4.X versions.

## [1.3.4] - 2023-11-27
### Fixed
- Fixed an issue with previous value in MultiStateManagerBase.

## [1.3.3] - 2023-11-27
### Fixed
- Fixed minor issue with MultiStateListener.

## [1.3.2] - 2023-11-27
### Added
- Adding MultiStateListener classes.

## [1.3.1] - 2023-11-17
### Added
- Adding shortcut methods to MultiStateManager

## [1.3.0] - 2023-11-16
### Added
- Multi-State Manager & Listener

## [1.2.1] - 2022-11-08
### Added
- State Transition Listener
- State Listener runtime debug list (Odin only)
- State selection based on referenced state manager (Odin only)
### Updated
- Set to Unity 2021.3
- Changed names of basic state objects to simpler versions.
### Removed
- Active and Inactive responses for State Listeners

## [1.2.0] - 2022-11-01
### Added
- Support for NaughtyAttributes.

## [1.1.0] - 2022-04-19
### Added
- New entering and leaving state responses added to StateListenerBase.
- Previous state now tracked and sent with state change events.

## [1.0.0] - 2022-01-14
### Added
- Initial creation of Unity-State.