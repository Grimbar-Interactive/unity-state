# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/)

==
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