#include <QGuiApplication>
#include <QQmlApplicationEngine>
#include <qtest1.h>
#include <google/protobuf/wire_format_lite.h>
#include <roc-lib/qrocobjectadapter.h>

int main(int argc, char *argv[])
{
    QCoreApplication::setAttribute(Qt::AA_EnableHighDpiScaling);

    QGuiApplication app(argc, argv);

    QQmlApplicationEngine engine;

    QRocObjectAdapter::setSharedChannel(grpc::CreateChannel("localhost:8000", grpc::InsecureChannelCredentials()));
    qmlRegisterType<Tests::QTest1>("interop", 1, 0, "Test1");

    const QUrl url(QStringLiteral("qrc:/main.qml"));
    QObject::connect(&engine, &QQmlApplicationEngine::objectCreated,
                     &app, [url](QObject *obj, const QUrl &objUrl) {
        if (!obj && url == objUrl)
            QCoreApplication::exit(-1);
    }, Qt::QueuedConnection);

    engine.load(url);

    return app.exec();
}
