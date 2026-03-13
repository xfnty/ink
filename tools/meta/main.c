#include <io.h>
#include <fcntl.h>
#include <stdio.h>
#include <stdlib.h>
#include <stdint.h>
#include <sys/stat.h>
#include <sys/types.h>

#include <io.h>
#include <fcntl.h>
#include <time.h>
#include <sys/types.h>
#include <sys/stat.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <errno.h>
#include <share.h>

#define LONESHA256_IMPLEMENTATION
#include "lonesha256.h"

const char *template =
    "{\n"
    "  \"Name\": \"Windows Ink\",\n"
    "  \"Owner\": \"xfnty\",\n"
    "  \"Description\": \"Sends WM_POINTER messages to active window with tilt and pressure data using Windows Pointer API.\\nThis plugin was created to make PaintTool SAI work with OpenTabletDriver.\",\n"
    "  \"PluginVersion\": \"%s\",\n"
    "  \"SupportedDriverVersion\": \"0.6.1.0\",\n"
    "  \"RepositoryUrl\": \"https://github.com/xfnty/ink\",\n"
    "  \"DownloadUrl\": \"https://github.com/xfnty/ink/releases/download/latest/plugin.zip\",\n"
    "  \"CompressionFormat\": \"zip\",\n"
    "  \"SHA256\": \"%s\",\n"
    "  \"WikiUrl\": \"https://github.com/xfnty/ink\",\n"
    "  \"LicenseIdentifier\": \"Unlicense\"\n"
    "}\n";

int main(int argc, const char **argv) {
    if (argc != 4) {
        fprintf(stderr, "Usage: meta <plugin-dll> <plugin-version> <output-metadata-path>\n");
        return 1;
    }

    int plugin, metadata;

    _sopen_s(&plugin, argv[1], _O_BINARY | _O_RDONLY | _O_SEQUENTIAL, _SH_DENYWR, _S_IREAD);
    if (plugin == -1) {
        fprintf(stderr, "error: failed to open \"%s\": %s\n", argv[1], strerror(errno));
        return errno;
    }

    struct _stat64 plugin_stat;
    if (_fstat64(plugin, &plugin_stat) != 0) {
        fprintf(stderr, "error: failed to get size of \"%s\": %s\n", argv[1], strerror(errno));
        return errno;
    }

    char *plugin_bytes = malloc(plugin_stat.st_size);
    if (_read(plugin, plugin_bytes, plugin_stat.st_size) != plugin_stat.st_size) {
        fprintf(stderr, "error: failed to read \"%s\": %s\n", argv[1], strerror(errno));
        return errno;
    }

    unsigned char hash_bytes[32];
    lonesha256(hash_bytes, plugin_bytes, plugin_stat.st_size);
    for (int i = 0; i < sizeof(hash_bytes) / 2; i++) {
        int t = hash_bytes[i];
        hash_bytes[i] = hash_bytes[sizeof(hash_bytes) - 1 - i];
        hash_bytes[sizeof(hash_bytes) - 1 - i] = t;
    }

    char hash_text[65] = { '\0' };
    snprintf(
        hash_text,
        sizeof(hash_text),
        "%08llx%08llx%08llx%08llx",
        *(uint64_t*)(hash_bytes + 24),
        *(uint64_t*)(hash_bytes + 16),
        *(uint64_t*)(hash_bytes + 8),
        *(uint64_t*)(hash_bytes)
    );

    _sopen_s(&metadata, argv[3], _O_BINARY | _O_CREAT | _O_TRUNC | _O_WRONLY, _SH_DENYRD, _S_IWRITE);
    if (!metadata) {
        fprintf(stderr, "error: failed to create \"%s\": %s\n", argv[3], strerror(errno));
        return errno;
    }

    char version[256];
    int versionLen = strlen(argv[2]);
    if (versionLen > 0) {
        strncpy(version, argv[2], versionLen);
    } else {
        versionLen = 5;
        strncpy(version, "0.1.0", versionLen);
    }

    char text[4096];
    int textlen = snprintf(text, sizeof(text), template, version, hash_text);
    _write(metadata, text, textlen);

    return 0;
}
