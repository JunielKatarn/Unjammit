#!/usr/bin/env bash

echo "APPCENTER_BUILD_ID [$APPCENTER_BUILD_ID]"
echo "APPCENTER_BRANCH [$APPCENTER_BRANCH]"
echo "APPCENTER_SOURCE_DIRECTORY [$APPCENTER_SOURCE_DIRECTORY]"
echo "APPCENTER_OUTPUT_DIRECTORY [$APPCENTER_OUTPUT_DIRECTORY]"
echo "APPCENTER_TRIGGER [$APPCENTER_TRIGGER]"

if [ -n "$UPDATE_VERSION" ]; then
	echo "UNJAMMIT_BASE_VERSION [$UNJAMMIT_BASE_VERSION]"
	echo "BUNDLE_ID             [$BUNDLE_ID]"
	echo "BUNDLE_NAME           [$BUNDLE_NAME]"
	echo "DISPLAY_NAME          [$DISPLAY_NAME]"

	/usr/libexec/PlistBuddy -c "Set :CFBundleIdentifier ${BUNDLE_ID}" Info.plist
	/usr/libexec/PlistBuddy -c "Set :CFBundleName ${BUNDLE_NAME}" Info.plist
	/usr/libexec/PlistBuddy -c "Set :CFBundleDisplayName ${DISPLAY_NAME}" Info.plist
	/usr/libexec/PlistBuddy -c "Set :CFBundleVersion ${UNJAMMIT_BASE_VERSION}.${APPCENTER_BUILD_ID}" Info.plist
	/usr/libexec/PlistBuddy -c "Set :CFBundleShortVersionString ${UNJAMMIT_BASE_VERSION}.${APPCENTER_BUILD_ID}" Info.plist
fi
