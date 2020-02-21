#include "qtest2.h"
#include "qtest2worker.h"
#include <QThread>
#include <QMap>
#include <QJSValue>
#include <QSharedPointer>
#include <QQmlEngine>
#include <QQmlContext>
using namespace Tests;
struct CallbackRequest
{
	QJSValue state;
	QJSValue callback;
};
class Tests::QTest2Private
{
public:
	QTest2Private() : currentRequestId(0) {}
	QThread workerThread;
	QTest2Worker* worker;
	QMap<int, QSharedPointer<CallbackRequest>> requests;
	int currentRequestId;
};
QTest2::QTest2(QObject* parent) : QObject(parent), d_priv(new Tests::QTest2Private())
{
	d_priv->worker = new QTest2Worker();
	d_priv->worker->moveToThread(&d_priv->workerThread);
	connect(&d_priv->workerThread, SIGNAL(finished()), d_priv->worker, SLOT(deleteLater()));
	d_priv->workerThread.start();
}
QTest2::~QTest2()
{
	d_priv->workerThread.quit();
	d_priv->workerThread.wait();
}
